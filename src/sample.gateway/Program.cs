namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

class Program
{
    protected Program() { }

    public static int Main(string[] args)
    {
        Console.WriteLine($"SampleGateway {DateTime.UtcNow:r}");

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddConsole();
            builder.AddEventSourceLogger();
        });

        ILogger logger = loggerFactory.CreateLogger("Startup");

        IHost host = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configBuilder =>
            {
                // these should be set in the LaunchSettings.json or at runtime in Environment Variables
                string coreEnvironment = Environment.GetEnvironmentVariable("CS_ENVIRONMENT");
                string coreClusterCategory = Environment.GetEnvironmentVariable("CS_CATEGORY");

                configBuilder
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{coreEnvironment}.json".ToLower(), optional: true, reloadOnChange: true)
                    .AddJsonFile($"appsettings.{coreEnvironment}-{coreClusterCategory}.json".ToLower(), optional: true, reloadOnChange: true);
            })
            .ConfigureLogging(lb =>
            {
                lb.ClearProviders();
                lb.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
                lb.AddConsole();
                lb.AddDebug();
            })
            .ConfigureServices((context, services) =>
            {
                services
                    .AddSingleton(context.Configuration)
                    .AddLoggers(logger, loggerFactory)
                    .AddAuthentication()
                    .AddNeptuneDiscovery(context.Configuration);
            })
            .Build();

        IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
        logger.LogInformation("Running in config {ClusterCategory}", config.GetValue<string>("Gateway:ClusterCategory"));

        RootCommand rootCommand = new("Power Platform Licensing Sample Gateway");
        rootCommand.Subcommands.Add(CommandAllocationEnvironmentGetOptions.BuildCommandAllocationEnvironmentGet(host, logger));
        rootCommand.Subcommands.Add(CommandAllocationEnvironmentPatchOptions.BuildCommandAllocationEnvironmentPatch(host, logger));
        rootCommand.Subcommands.Add(CommandAllocationEnvironmentsPatchOptions.BuildCommandAllocationEnvironmentsPatch(host, logger));
        rootCommand.Subcommands.Add(CommandBillingPoliciesGetOptions.BuildCommandBillingPoliciesGet(host, logger));
        rootCommand.Subcommands.Add(CommandBillingPolicyEnvironmentGetOptions.BuildCommandBillingPolicyEnvironmentGet(host, logger));
        rootCommand.Subcommands.Add(CommandCapacityGetOptions.BuildCommandCapacityGet(host, logger));
        rootCommand.Subcommands.Add(CommandEnvironmentBillingGetOptions.BuildCommandEnvironmentBillingGet(host, logger));
        rootCommand.Subcommands.Add(CommandSdkCapacityGetOptions.BuildCommandSdkCapacityGet(host, logger));

        return rootCommand.Parse(args).Invoke();
    }
}
