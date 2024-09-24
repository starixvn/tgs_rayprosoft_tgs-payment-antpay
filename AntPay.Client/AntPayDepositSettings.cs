using System.ComponentModel;

namespace Tgs.Payment.AntPay.Client;
public record AntPayDepositSettings(
    string MerchantNo,
    string ApiKey,
    Guid FormId,    
    int Source,    
    int PayType,
    string? ChannelCode,
    string PlayerCode,
    string? RealName,
    string UserIp,
    string DeviceType);