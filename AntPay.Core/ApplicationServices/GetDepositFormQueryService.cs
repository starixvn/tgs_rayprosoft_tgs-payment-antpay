using System.Text.Json;
using Tgs.Payment.AntPay.Client;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AntPay.ApplicationServices;

internal class GetDepositFormQueryService(
    IPaymentServiceClient paymentServiceClient) : IGetService<GetDepositFormQuery, DepositForm?>
{
    public async ValueTask<DepositForm?> GetAsync(GetDepositFormQuery query, CancellationToken cancellationToken = default)
    {
        var data = await paymentServiceClient
            .GetDepositFormAsync(query.FormId, cancellationToken)
        .ConfigureAwait(false);

        return data == null
            ? null
            : new DepositForm(
                data.Id,
                data.PlayerCode,
                data.BrandId,
                data.BrandPaymentId,
                data.Amount,
                data.Status,
                string.IsNullOrEmpty(data.ApplyInfo)
                    ? null
                    : JsonSerializer.Deserialize<AntPayDepositApplyInfo>(data.ApplyInfo, AntPayUtility.SerializerOptions), data.PaymentReturn);
    }
}