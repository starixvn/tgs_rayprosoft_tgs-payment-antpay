using System.Net.Http.Headers;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using NSubstitute;
using Tgs.Payment.AntPay.Client;
using Tgs.Payment.Clients;
using Xunit.Abstractions;

namespace Tgs.Payment.AntPay.IntegrationTests;

public class DepositTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task 代收回調_訂單狀態成功()
    {
        var fakePaymentServiceClient = Substitute.For<IPaymentServiceClient>();
        var fakePaymentClient = Substitute.For<IPaymentClient>();

        await using var web = AntPayTestHost.Factory
            .WithWebHostBuilder(hostBuilder => hostBuilder
                .ConfigureLogging(logging => logging.AddXUnit(testOutputHelper))
                .ConfigureServices(services => services
                    .AddSingleton(fakePaymentServiceClient)
                    .AddSingleton(fakePaymentClient)));

        var httpClient = web.CreateClient();

        var formId = Guid.Parse("db8a2657-66b8-41f8-9734-21b9c457b916");
        var brandId = Guid.NewGuid();
        var apiKey = "817ec927b243a3141ed5ec3277d15591";

        var request = """
            {
                "randomStr": "eWxFL7uJd2l",
                "orderNo": "P12878273710421237765900",
                "merOrderNo": "db8a2657-66b8-41f8-9734-21b9c457b916",
                "payTime": "2024-09-23 17:28:42",
                "sign": "bf90ad74e6dde6a22c22936e90df8274",
                "orderAmt": "5000.000",
                "userName": "4ucmqplxb029",
                "payAmt": "5000.000",
                "payBankName": "YVAWsB",
                "merchantNo": "chat_pay_test4",
                "status": "100"
            }
            """;

        var content = new StringContent(request, Encoding.UTF8, new MediaTypeHeaderValue(MediaTypeNames.Application.Json));

        var applyInfo = new AntPayDepositApplyInfo(
            new AntPayDepositSettings(
                "chat_pay_test4",
                apiKey,
                formId,
                Source: 30,
                PayType: 65,
                "YVAWsB",
                "4ucmqplxb029",
                "testName",
                "127.0.0.1",
                "PC"),
             new AntPayDepositResponse(
                5000m,
                formId,
                new Uri("https://jump.ff1555.com/cc?data=eyJyZWNBY2N0Tm8iOiIwMTIzNDU2Nzg5IiwiY29kZSI6IjEiLCJyZWNCYW5rQnJhbmNoIjoi5YyX5Lqs5pSv6KGMIiwidXNlclN0YXR1cyI6MSwicmVjQmFua05hbWUiOiLlt6XllYbpk7booYwiLCJvcmRlckFtdCI6NTAwMCwiZXJyb3IiOmZhbHNlLCJtZXNzYWdlIjoi5oiQ5YqfIiwiZXhpc3RJblBheU9yZGVyIjpmYWxzZSwicmVjQWNjdE5hbWUiOiLmg7PpkrHmg7Pnlq_kuoYiLCJzdWNjZXNzZnVsIjp0cnVlLCJ0cyI6IjE3MjcwODM1NzE5ODEifQ=="),
                "db8a2657-66b8-41f8-9734-21b9c457b916",
                "0123456789",
                "想钱想疯了",
                "工商银行",
                "北京支行",
                false));

        _ = fakePaymentServiceClient.GetDepositFormAsync(Arg.Is(formId), Arg.Any<CancellationToken>())
            .Returns(new DepositFormInfo
            {
                Id = formId,
                BrandId = brandId,
                Status = PaymentFlowState.WaitPayment,
                Amount = 5000m,
                ThirdPartyOrderNo = "db8a2657-66b8-41f8-9734-21b9c457b916",
                ApplyInfo = JsonSerializer.Serialize(applyInfo, AntPayUtility.SerializerOptions)
            });

        var response = await httpClient.PostAsync("/deposit/callback", content);

        var actual = await response.Content.ReadAsStringAsync();

        Assert.Equal("""
            {"code":1,"message":"成功"}
            """, actual);

        var paymentReturn = new AntPayDepositReturn()
        {
            Status = "100",
            FormId = formId,
            OrderAmount = decimal.Parse("5000.000"),
            Sign = "bf90ad74e6dde6a22c22936e90df8274",
            MerchantNo = "chat_pay_test4",
            ThirdPartyOrderNo = "P12878273710421237765900",
            PayTime = DateTime.ParseExact("2024-09-23 17:28:42", "yyyy-MM-dd HH:mm:ss", null),
            PayAmount = decimal.Parse("5000.000"),
            PayBankName = "YVAWsB",
            UserName = "4ucmqplxb029"
        };

        _ = fakePaymentServiceClient.Received(1)
            .SubmitDepositFormStatusAsync(
                Arg.Is(formId),
                paymentReturn: Arg.Is(JsonSerializer.Serialize(paymentReturn, AntPayUtility.SerializerOptions)),
                cancellationToken: Arg.Any<CancellationToken>());

        _ = fakePaymentClient.Received(1)
            .ApproveDepositAsync(
                Arg.Is(formId),
                Arg.Is(brandId),
                Arg.Is(paymentReturn.PayAmount),
                Arg.Any<CancellationToken>());
    }
}