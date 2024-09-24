namespace Tgs.Payment.AntPay;
public record DepositOrder(
    decimal Amount,
    Guid FormId,
    Uri CallbackUrl,
    int Source,
    int PayType,
    string? ChannelCode,
    string PlayerCode,
    string? RealName,
    string UserIp,
    string DeviceType);