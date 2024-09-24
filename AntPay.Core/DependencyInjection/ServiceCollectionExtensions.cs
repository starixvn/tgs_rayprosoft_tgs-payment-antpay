using System.Reflection;
using Tgs.Payment.AntPay;

namespace Microsoft.Extensions.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddAntPay(
        this IServiceCollection services,
        Action<AntPayOptions, IServiceProvider> configureOptions,
        Action<IHttpClientBuilder> ConfigureAiClient)
    {
        ConfigureAiClient?.Invoke(services.AddHttpClient<AntPayClient>());

        return services
            .AddOptions<AntPayOptions>()
            .Configure(configureOptions)
            .Services
            .RegisterAllServiceUtilityTypes(Assembly.GetExecutingAssembly(), ServiceLifetime.Transient)
            .AddSingleton<AntPayHasher>();
    }
}