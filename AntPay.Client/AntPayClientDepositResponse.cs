using System.ComponentModel;

namespace Tgs.Payment.AntPay.Client;

public record AntPayClientDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo,
    [Description("收款卡卡号 银行卡转账返回")]
    string? RecAcctNo,
    [Description("收款卡户名 银行卡转账返回")]
    string? RecAcctName,
    [Description("收款卡银行名称 银行卡转账返回")]
    string? RecBankName,
    [Description("收款卡分行信息 银行卡转账返回")]
    string? RecBankBranch,
    [Description("是否是支付中订单 银行卡转账返回")]
    bool? ExistInPayOrder);