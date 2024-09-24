using System.Text.Json.Serialization;

namespace Tgs.Payment.AntPay.ViewModels;

public class CallbackDepositViewModel
{
    /// <summary>
    /// 商户号，平台分配
    /// </summary>
    [JsonPropertyName("merchantNo")]
    public required string MerchantNo { get; set; }

    /// <summary>
    /// 商户订单号
    /// </summary>
    [JsonPropertyName("merOrderNo")]
    public required string MerOrderNo { get; set; }

    /// <summary>
    /// 平台订单号
    /// </summary>
    [JsonPropertyName("orderNo")]
    public required string OrderNo { get; set; }

    /// <summary>
    /// 订单金额
    /// </summary>
    [JsonPropertyName("orderAmt")]
    public required string OrderAmt { get; set; }

    /// <summary>
    /// 商户获取聊天地址接口时提交
    /// </summary>
    [JsonPropertyName("userName")]
    public required string UserName { get; set; }

    /// <summary>
    /// 用户支付金额 状态为100时返回，请务必按这个金额给用户上分
    /// </summary>
    [JsonPropertyName("payAmt")]
    public required string PayAmt { get; set; }

    [JsonPropertyName("payBankName")]
    public required string PayBankName { get; set;}

    /// <summary>
    /// 平台订单完成时间
    /// </summary>
    [JsonPropertyName("payTime")]
    public required string PayTime {  get; set; }

    /// <summary>
    /// 100成功 -100失败
    /// </summary>
    [JsonPropertyName("status")]
    public required string Status { get; set; }

    /// <summary>
    /// 随机字符串
    /// </summary>
    [JsonPropertyName("randomStr")]
    public required string RandomStr { get; set; }

    [JsonPropertyName("sign")]
    public required string Sign { get; set; }
}