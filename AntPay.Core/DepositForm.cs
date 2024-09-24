using Tgs.Payment.AntPay.Client;
using Tgs.Payment.Clients;

namespace Tgs.Payment.AntPay;
public record DepositForm(
    Guid Id,
    string PlayerCode,
    Guid BrandId,
    Guid BrandPaymentId,
    decimal Amount,
    PaymentFlowState Status,
    AntPayDepositApplyInfo? ApplyInfo,
    string? PaymentReturn = null);