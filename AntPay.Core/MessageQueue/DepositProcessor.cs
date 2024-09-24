using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Tgs.Payment.AntPay.Client;
using Tgs.Payment.Protos;
using Valhalla.MessageQueue;

namespace Tgs.Payment.AntPay.MessageQueue;

internal class DepositProcessor(
    AntPayClient antPayClient,
    IOptions<AntPayOptions> optionsAccessor,
    ILogger<DepositProcessor> logger)
    : IMessageProcessor<DepositRequest, DepositResponse>
{
    private readonly AntPayOptions _options = optionsAccessor.Value;

    public async ValueTask<DepositResponse> HandleAsync(
        string subject,
        DepositRequest data,
        IEnumerable<MessageHeaderValue>? headerValues,
        CancellationToken cancellationToken = default)
    {
        logger.LogInformation("{DepositApplyInfo}", data.ApplyInfo);

        var settings = JsonSerializer.Deserialize<AntPayDepositSettings>(
            data.ApplyInfo,
            AntPayUtility.SerializerOptions)!;

        var order = new DepositOrder(
            decimal.Parse(data.Amount),
            settings.FormId,
            new Uri($"{_options.CallbackBaseUrl}/deposit/callback"),
            settings.Source,
            settings.PayType,
            settings.ChannelCode,
            settings.PlayerCode,
            settings.RealName,
            settings.UserIp,
            settings.DeviceType);

        var depositResponse = await antPayClient
            .DepositAsync(settings.MerchantNo, settings.ApiKey, order, cancellationToken)
            .ConfigureAwait(false);

        var applyInfo = new AntPayDepositApplyInfo(
            settings,
            new AntPayDepositResponse(
                depositResponse.Amount,
                depositResponse.FormId,
                depositResponse.RedirectUrl,
                depositResponse.ThirdPartyOrderNo,
                depositResponse.RecAcctNo,
                depositResponse.RecAcctName,
                depositResponse.RecBankName,
                depositResponse.RecBankBranch,
                depositResponse.ExistInPayOrder));

        return new DepositResponse
        {
            Result = JsonSerializer.Serialize(applyInfo, AntPayUtility.SerializerOptions)
        };
    }
}