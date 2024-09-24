namespace Tgs.Payment.AntPay.Client;
public record AntPayDepositReturn
{
    public decimal OrderAmount { get; init; }
    public Guid FormId { get; init; }

    public string Status { get; init; } = default!;

    public string Sign { get; init; } = default!;

    public string MerchantNo { get; init; } = default!;
    public string ThirdPartyOrderNo { get; init; } = default!;

    public string UserName { get; init; } = default!;

    public DateTime PayTime { get; init; }

    public decimal PayAmount { get; init; }

    public string PayBankName { get; init; } = default!;
}