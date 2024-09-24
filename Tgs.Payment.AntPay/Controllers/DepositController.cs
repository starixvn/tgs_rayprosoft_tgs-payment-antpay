using System.Net.Mime;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Tgs.Payment.AntPay.ApplicationServices;
using Tgs.Payment.AntPay.Client;
using Tgs.Payment.AntPay.ViewModels;
using Tgs.Payment.Clients;
using Tgs.ServiceUtility;

namespace Tgs.Payment.AntPay.Controllers;

[Route("[controller]")]
[ApiController]
public class DepositController(ILogger<DepositController> logger) : ControllerBase
{
    [Consumes(MediaTypeNames.Application.Json)]
    [Produces(MediaTypeNames.Application.Json)]
    [HttpPost("callback")]
    public async Task<object> DepositCallbackAsync(
       [FromServices] IGetService<GetDepositFormQuery, DepositForm?> getService,
       [FromServices] IPaymentServiceClient paymentServiceClient,
       [FromServices] IPaymentClient paymentClient,
       [FromServices] AntPayHasher hasher,
       [FromBody] CallbackDepositViewModel source)
    {
        logger.LogInformation("{source}", JsonSerializer.Serialize(source));
        
        var formId = Guid.Parse(source.MerOrderNo);

        var depositForm = await getService
            .GetAsync(new(formId), HttpContext.RequestAborted)
            .ConfigureAwait(false);

        if (depositForm == null)
        {
            logger.LogWarning("Can't found deposit form({id}).", formId);
            return "fail";
        }

        var apiKey = depositForm.ApplyInfo!.Settings.ApiKey;

        var dict = JsonSerializer.Deserialize<Dictionary<string, object>>(JsonSerializer.Serialize(source))!;

        var sign = source.Sign;
        var hash = hasher.ComputeHash(
            apiKey,
            dict.Select(kv => new KeyValuePair<string, string?>(kv.Key, kv.Value.ToString())));

        logger.LogInformation("Compute hash: {hash}, sign: {sign}", hash, sign);

        if (sign != hash)
        {
            logger.LogWarning("Sign value invalid. input: {sign}, compute: {hash}", sign, hash);
            return "fail";
        }

        var paymentReturn = new AntPayDepositReturn()
        {
            OrderAmount = decimal.Parse(source.OrderAmt),
            FormId = formId,
            Status = source.Status,
            Sign = source.Sign,
            MerchantNo = source.MerchantNo,
            ThirdPartyOrderNo = source.OrderNo,
            PayTime = DateTime.ParseExact(source.PayTime, "yyyy-MM-dd HH:mm:ss", null),
            UserName = source.UserName,
            PayAmount = decimal.Parse(source.PayAmt),
            PayBankName = source.PayBankName,
        };

        var fsm = PaymentStateMachineFactory.CreateDepositStateMachine(depositForm.Status);

        switch (paymentReturn.Status)
        {
            case "100"://成功
                if (fsm.CanFire(PaymentTrigger.PaymentDepositCompleted))
                {
                    await fsm.FireAsync(PaymentTrigger.PaymentDepositCompleted).ConfigureAwait(false);

                    await paymentServiceClient.SubmitDepositFormStatusAsync(
                        depositForm.Id,
                        paymentReturn: JsonSerializer.Serialize(paymentReturn, AntPayUtility.SerializerOptions),
                        cancellationToken: HttpContext.RequestAborted).ConfigureAwait(false);

                    await paymentClient.ApproveDepositAsync(
                        depositForm.Id,
                        depositForm.BrandId,
                        paymentReturn.PayAmount,
                        HttpContext.RequestAborted).ConfigureAwait(false);
                }

                break;

            default:
                break;
        }
        
        return new
        {
            code = 1,
            message = "成功"
        };
    }
}