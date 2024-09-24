using Microsoft.Extensions.Logging.Abstractions;
using Tgs.Payment.AntPay;

namespace AntPay.Core.UnitTests;

public class HasherTests
{
    [Fact]
    public void 訂單查詢()
    {
        var apiKey = "apiKey";

        var dict = new Dictionary<string, string?>
        {
            ["merchantNo"] = "merchantNo",
            ["randomStr"] = "1727080524",
            ["merOrderNo"] = "258ca647-90f3-45b4-b7ed-4837f6c5e31c",
        };
        var expected = "a843dac251c96caa610d2bd976c966cc";

        var sut = new AntPayHasher(NullLogger<AntPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void 代收回調()
    {
        var apiKey = "817ec927b243a3141ed5ec3277d15591";

        var dict = new Dictionary<string, string?>
        {
            ["randomStr"] = "eWxFL7uJd2l",
            ["orderNo"] = "P12878273710421237765900",
            ["merOrderNo"] = "db8a2657-66b8-41f8-9734-21b9c457b916",
            ["payTime"] = "2024-09-23 17:28:42",
            ["sign"] = "bf90ad74e6dde6a22c22936e90df8274",
            ["orderAmt"] = "5000.000",
            ["userName"] = "4ucmqplxb029",
            ["payAmt"] = "5000.000",
            ["payBankName"] = "YVAWsB",
            ["merchantNo"] = "chat_pay_test4",
            ["status"] = "100"
        };

        var expected = "bf90ad74e6dde6a22c22936e90df8274";

        var sut = new AntPayHasher(NullLogger<AntPayHasher>.Instance);

        var actual = sut.ComputeHash(apiKey, dict);

        Assert.Equal(expected, actual);
    }
}