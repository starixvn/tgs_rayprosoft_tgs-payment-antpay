using System.Net;
using System.Text.Json;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using RichardSzalay.MockHttp;
using Tgs.Payment.AntPay;
using Tgs.Payment.AntPay.Client;
using Tgs.Payment.AntPay.MessageQueue;
using Tgs.Payment.Clients;
using Tgs.Payment.Protos;

namespace AntPay.Core.UnitTests;

public class DepositProcessorTests
{
    [Fact]
    public async Task 儲值要求處理()
    {
        // Arrange
        var mockHttp = new MockHttpMessageHandler();

        var httpClient = mockHttp.ToHttpClient();
        httpClient.BaseAddress = new Uri("https://localhost");

        var fakePayClient = new AntPayClient(
            httpClient,
            new AntPayHasher(NullLogger<AntPayHasher>.Instance),
            NullLogger<AntPayClient>.Instance);

        var fakePaymentClient = Substitute.For<IPaymentClient>();
        var options = new AntPayOptions
        {
            CallbackBaseUrl = new Uri("https://localhost"),
            PaymentApiUrl = new Uri("https://localhost"),
            PaymentServiceBaseUrl = new Uri("http://localhost:5541")
        };
        var formId = Guid.Parse("db8a2657-66b8-41f8-9734-21b9c457b916");
        var merchantNo = "merchantNo";
        var apiKey = "apiKey";

        var sut = new DepositProcessor(
            fakePayClient,
            new OptionsWrapper<AntPayOptions>(options),
            NullLogger<DepositProcessor>.Instance);

        var applySettings = new AntPayDepositSettings(
            merchantNo,
            apiKey,
            formId,
            Source: 30,
            PayType: 65,
            ChannelCode: "channelCode",
            "4ucmqplxb029",
            "testName",
            "127.0.0.1",
            "PC");

        var request = new DepositRequest
        {
            Amount = "5000",
            ApplyInfo = JsonSerializer.Serialize(applySettings, AntPayUtility.SerializerOptions)
        };

        _ = mockHttp.Expect(HttpMethod.Post, "https://localhost/api/pay/public/api/pay/create")
            .Respond(HttpStatusCode.OK, new StringContent("""
                {"code":"1","message":"成功","payUrl":"https://jump.ff1555.com/cc?data=eyJyZWNBY2N0Tm8iOiIwMTIzNDU2Nzg5IiwiY29kZSI6IjEiLCJyZWNCYW5rQnJhbmNoIjoi5YyX5Lqs5pSv6KGMIiwidXNlclN0YXR1cyI6MSwicmVjQmFua05hbWUiOiLlt6XllYbpk7booYwiLCJvcmRlckFtdCI6NTAwMCwiZXJyb3IiOmZhbHNlLCJtZXNzYWdlIjoi5oiQ5YqfIiwiZXhpc3RJblBheU9yZGVyIjpmYWxzZSwicmVjQWNjdE5hbWUiOiLmg7PpkrHmg7Pnlq_kuoYiLCJzdWNjZXNzZnVsIjp0cnVlLCJ0cyI6IjE3MjcwODM1NzE5ODEifQ==","recAcctNo":"0123456789","recAcctName":"想钱想疯了","recBankName":"工商银行","recBankBranch":"北京支行","orderAmt":5000,"existInPayOrder":false,"userStatus":1}
                """));

        // Act
        var actual = await sut.HandleAsync(
            "test",
            request,
            null);

        var applyInfo = JsonSerializer.Deserialize<AntPayDepositApplyInfo>(actual.Result, AntPayUtility.SerializerOptions);

        Assert.NotNull(applyInfo);
        Assert.Equal(applySettings, applyInfo.Settings);
        Assert.Equal(
            new AntPayDepositResponse(
                5000m,
                applySettings.FormId,
                new Uri("https://jump.ff1555.com/cc?data=eyJyZWNBY2N0Tm8iOiIwMTIzNDU2Nzg5IiwiY29kZSI6IjEiLCJyZWNCYW5rQnJhbmNoIjoi5YyX5Lqs5pSv6KGMIiwidXNlclN0YXR1cyI6MSwicmVjQmFua05hbWUiOiLlt6XllYbpk7booYwiLCJvcmRlckFtdCI6NTAwMCwiZXJyb3IiOmZhbHNlLCJtZXNzYWdlIjoi5oiQ5YqfIiwiZXhpc3RJblBheU9yZGVyIjpmYWxzZSwicmVjQWNjdE5hbWUiOiLmg7PpkrHmg7Pnlq_kuoYiLCJzdWNjZXNzZnVsIjp0cnVlLCJ0cyI6IjE3MjcwODM1NzE5ODEifQ=="),
                "db8a2657-66b8-41f8-9734-21b9c457b916",
                "0123456789",
                "想钱想疯了",
                "工商银行",
                "北京支行",
                false),
            applyInfo.Response);
    }
}