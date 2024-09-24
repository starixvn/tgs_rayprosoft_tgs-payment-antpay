namespace Tgs.Payment.AntPay.Client;
public record AntPayDepositApplyInfo(
    AntPayDepositSettings Settings,
    AntPayDepositResponse Response);