using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Logging;
using Tgs.Payment.AntPay.Client;

namespace Tgs.Payment.AntPay;

public class AntPayClient(
    HttpClient httpClient,
    AntPayHasher hasher,
    ILogger<AntPayClient> logger)
{
    public async ValueTask<AntPayClientDepositResponse> DepositAsync(
        string merchantNo,
        string apiKey,
        DepositOrder order,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merchantNo"] = merchantNo,
            ["merOrderNo"] = order.FormId.ToString(),
            ["orderAmt"] = order.Amount.ToString("#.##"),
            ["source"] = order.Source.ToString(),
            ["payType"] = order.PayType.ToString(),
            ["userName"] = order.PlayerCode,
            ["notifyUrl"] = order.CallbackUrl.AbsoluteUri,            
            ["userIp"] = order.UserIp,
            ["deviceType"] = order.DeviceType,
            ["randomStr"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
        };

        if (order.ChannelCode is not null)
            dict["channelCode"] = order.ChannelCode;

        if (order.RealName is not null)
            dict["trueName"] = order.RealName;

        var sign = hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await httpClient
            .PostAsJsonAsync("/api/pay/public/api/pay/create", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Deposit response can't read.");

        if (node["code"]!.GetValue<string>() != "1")
        {
            logger.LogWarning("AntPay Deposit Failed");
            throw new ApiFailException(node["message"]!.ToString());
        }

        return new AntPayClientDepositResponse(
            order.Amount,
            order.FormId,
            RedirectUrl: new Uri(node["payUrl"]!.GetValue<string>()),
            ThirdPartyOrderNo: order.FormId.ToString(),
            RecAcctNo: node["recAcctNo"]?.GetValue<string>(),
            RecAcctName: node["recAcctName"]?.GetValue<string>(),
            RecBankName: node["recBankName"]?.GetValue<string>(),
            RecBankBranch: node["recBankBranch"]?.GetValue<string>(),
            ExistInPayOrder: node["existInPayOrder"]?.GetValue<bool>());
    }

    public async ValueTask<int> QueryOrderAsync(
        string merchantNo,
        string apiKey,
        Guid formId,
        CancellationToken cancellationToken = default)
    {
        var dict = new Dictionary<string, string?>
        {
            ["merchantNo"] = merchantNo,
            ["merOrderNo"] = formId.ToString(),
            ["randomStr"] = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(),
        };

        var sign = hasher.ComputeHash(apiKey, dict);

        dict["sign"] = sign;

        var response = await httpClient
            .PostAsJsonAsync($"/api/pay/public/api/pay/query", dict, cancellationToken)
            .ConfigureAwait(false);

        _ = response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);

        var node = JsonNode.Parse(body) ?? throw new ApiFailException("Query Order response can't read.");

        if (node["code"]!.GetValue<string>() != "1")
        {
            logger.LogWarning("AntPay Query Order Failed");
            throw new ApiFailException(node["message"]!.ToString());
        }

        //订单状态 100成功 -100失败 别的都算未完结
        return node["status"]!.GetValue<int>();
    }
}