using System.Net;
using Microsoft.Extensions.Logging.Abstractions;
using RichardSzalay.MockHttp;
using Tgs.Payment.AntPay;

namespace AntPay.Core.UnitTests;

public class AntPayClientTests
{
    [Fact]
    public async Task 代收_收銀台()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new AntPayHasher(NullLogger<AntPayHasher>.Instance);

        var sut = new AntPayClient(httpClient, signer, NullLogger<AntPayClient>.Instance);

        var formId = Guid.Parse("258ca647-90f3-45b4-b7ed-4837f6c5e31c");
        var merchantNo = "merchantNo";
        var apiKey = "apiKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/pay/public/api/pay/create")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {"code":"1","message":"成功","payUrl":"https://jump.ff1555.com/cc?data=eyJyZWNBY2N0Tm8iOiIwMTIzNDU2Nzg5IiwiY29kZSI6IjEiLCJyZWNCYW5rQnJhbmNoIjoi5YyX5Lqs5pSv6KGMIiwidXNlclN0YXR1cyI6MSwicmVjQmFua05hbWUiOiLlt6XllYbpk7booYwiLCJvcmRlckFtdCI6NTAwMCwiZXJyb3IiOmZhbHNlLCJtZXNzYWdlIjoi5oiQ5YqfIiwiZXhpc3RJblBheU9yZGVyIjpmYWxzZSwicmVjQWNjdE5hbWUiOiLmg7PpkrHmg7Pnlq_kuoYiLCJzdWNjZXNzZnVsIjp0cnVlLCJ0cyI6IjE3MjcwODAzNjg5MzIifQ==","recAcctNo":"0123456789","recAcctName":"想钱想疯了","recBankName":"工商银行","recBankBranch":"北京支行","orderAmt":5000,"existInPayOrder":false,"userStatus":1}
                """));

        var order = new DepositOrder(
           5000m,
           formId,
           new Uri("https://localhost"),
           30,
           65,
           "channelCode",
           "4ucmqplxb029",
           "testName",
           "127.0.0.1",
           "PC");

        var actual = await sut.DepositAsync(merchantNo, apiKey, order);

        Assert.Equal("258ca647-90f3-45b4-b7ed-4837f6c5e31c", actual.ThirdPartyOrderNo);
        Assert.Equal(new Uri("https://jump.ff1555.com/cc?data=eyJyZWNBY2N0Tm8iOiIwMTIzNDU2Nzg5IiwiY29kZSI6IjEiLCJyZWNCYW5rQnJhbmNoIjoi5YyX5Lqs5pSv6KGMIiwidXNlclN0YXR1cyI6MSwicmVjQmFua05hbWUiOiLlt6XllYbpk7booYwiLCJvcmRlckFtdCI6NTAwMCwiZXJyb3IiOmZhbHNlLCJtZXNzYWdlIjoi5oiQ5YqfIiwiZXhpc3RJblBheU9yZGVyIjpmYWxzZSwicmVjQWNjdE5hbWUiOiLmg7PpkrHmg7Pnlq_kuoYiLCJzdWNjZXNzZnVsIjp0cnVlLCJ0cyI6IjE3MjcwODAzNjg5MzIifQ=="), actual.RedirectUrl);
    }

    [Fact]
    public async Task 訂單查詢()
    {
        var mockHttp = new MockHttpMessageHandler();
        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");
        var signer = new AntPayHasher(NullLogger<AntPayHasher>.Instance);

        var sut = new AntPayClient(httpClient, signer, NullLogger<AntPayClient>.Instance);

        var formId = Guid.Parse("258ca647-90f3-45b4-b7ed-4837f6c5e31c");
        var merchantNo = "merchantNo";
        var apiKey = "apiKey";

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/pay/public/api/pay/query")
            .Respond(HttpStatusCode.OK, new StringContent("""
            {"code":"1","message":"成功","merchantNo":"chatpay_api_test","merOrderNo":"258ca647-90f3-45b4-b7ed-4837f6c5e31c","orderAmt":5000.000,"status":50,"ts":1727080525379}
            """));

        var actual = await sut.QueryOrderAsync(merchantNo, apiKey, formId);

        Assert.Equal(50, actual);
    }

}