namespace Tgs.Payment.AntPay.Client;
public record AntPayDepositResponse(
    decimal Amount,
    Guid FormId,
    Uri RedirectUrl,
    string ThirdPartyOrderNo,
    string? RecAcctNo,
    string? RecAcctName,
    string? RecBankName,
    string? RecBankBranch,
    bool? ExistInPayOrder);