namespace sample.gateway;

using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Identity.Client;
using sample.gateway.Discovery;
using sample.gateway.Tokens;

public static class StartupExtensions
{
    public static IServiceCollection AddLoggers(this IServiceCollection services, ILogger logger, ILoggerFactory loggerFactory)
    {
        services.TryAddSingleton<ILoggerFactory>(loggerFactory);
        services.TryAddSingleton(typeof(ILogger<>), typeof(Logger<>));
        services.TryAddSingleton<ILogger>(logger);
        return services;
    }

    public static IServiceCollection AddNeptuneDiscovery(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<INeptuneDiscovery, NeptuneDiscovery>();

        string coreEnvironment = Environment.GetEnvironmentVariable("CS_ENVIRONMENT");
        string coreClusterCategory = Environment.GetEnvironmentVariable("CS_CATEGORY");
        string coreClusterType = Environment.GetEnvironmentVariable("CS_TYPE");

        EvaluateConfigurationSet<GatewayConfig>(services, configuration, GatewayConfig.SectionName);
        EvaluateConfigurationSet<PowerPlatformEndpointsSettings>(services, configuration, PowerPlatformEndpointsSettings.SectionName);

        services.PostConfigure<GatewayConfig>(options =>
        {
            options.Environment = coreEnvironment;
            options.ClusterCategory = Enum.Parse<ClusterCategory>(coreClusterCategory);
            options.ClusterType = Enum.Parse<ClusterType>(coreClusterType);
        });

        return services;
    }

    private static void EvaluateConfigurationSet<T>(IServiceCollection services, IConfiguration configuration, string sectionName) where T : class
    {
        IConfigurationSection configSettingsOptions = configuration.GetSection(sectionName);

        if (configSettingsOptions == default || !configSettingsOptions.Exists())
        {
            throw new InvalidOperationException("Configuration is not set.");
        }

        services.Configure<T>(configSettingsOptions);
    }

    public static IServiceCollection AddAuthentication(this IServiceCollection services)
    {
        services.AddSingleton<IMsalHttpClientFactory, MsalRequestHttpClientFactory>();
        return services;
    }

}
