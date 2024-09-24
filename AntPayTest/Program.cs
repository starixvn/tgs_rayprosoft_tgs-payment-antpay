using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using NATS.Client.Core;
using Tgs.Payment.AntPay;
using Tgs.Payment.AntPay.Client;
using Tgs.Payment.Clients;

var builder = Host.CreateApplicationBuilder(args);

builder.Services
    .AddPaymentClient()
    .AddKeyedSingleton("Nats",
    (sp, _) => new NatsConnection(NatsOpts.Default with
    {
        Url = "nats://localhost:4222"
    }))
    .AddPhoenixToolkitsMessageQueue()
    .AddNatsMessageQueue(natsConfig => natsConfig
        .ConfigureResolveConnection(sp => sp.GetRequiredKeyedService<NatsConnection>("Nats")))
    .AddNatsGlobPatternExchange("*");

var app = builder.Build();

await app.StartAsync().ConfigureAwait(false);

var logger = app.Services.GetRequiredService<ILogger<Program>>();

var paymentClient = app.Services.GetRequiredService<IPaymentClient>();

var merchantNo = "chat_pay_test4";
var apiKey = "817ec927b243a3141ed5ec3277d15591";

//代收

var orderNo = Guid.NewGuid();
var amount = 5000.00m;

var depositsettings = new AntPayDepositSettings(
    merchantNo,
    apiKey,
    orderNo,
    30,
    65,
    "YVAWsB",
    "4ucmqplxb029",
    "testName",
    "127.0.0.1",
    "PC");

var depositreturn = await paymentClient.DepositAsync(
    "tgs.payment.antpay.deposit",
    amount,
    JsonSerializer.Serialize(depositsettings));
logger.LogInformation($"deposit return: {depositreturn}");


//訂單查詢
/*
var queryFormId = Guid.Parse("db8a2657-66b8-41f8-9734-21b9c457b916");

var httpClient = new HttpClient
{
    BaseAddress = new Uri("https://kfapi.ff1555.com")
};
var client = new AntPayClient(httpClient, new AntPayHasher(NullLogger<AntPayHasher>.Instance), NullLogger<AntPayClient>.Instance);
var orderQuery = await client.QueryOrderAsync(merchantNo, apiKey, queryFormId);

logger.LogInformation("{orderQuery}", orderQuery);*/