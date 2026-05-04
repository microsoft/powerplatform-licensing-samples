namespace sample.gateway;

using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

class Program
{
    private static ICommandOptions commandOptions { get; set; }
    protected Program() { }

    // Non-web application
    public static int Main(string[] args)
    {
        int result = 0;
        Console.WriteLine($"SampleGateway {DateTime.UtcNow:r}");

        // read through Verbs in all Options files
        Type[] commandVerbs = StartupExtensions.LoadVerbs();
        var argsAsList = new List<string>(args);
        ParserResult<object> parsedArgs = Parser.Default.ParseArguments(argsAsList, commandVerbs);
        ParserResult<object> options = parsedArgs
            .WithNotParsed<object>((errs) =>
            {
                bool continueProcessing = true;
                foreach (CommandLine.Error e in errs)
                {
                    Console.WriteLine("ERROR " + e.Tag.ToString());
                    continueProcessing = !e.StopsProcessing && continueProcessing;
                }
                if (!continueProcessing)
                {
                    return;
                }
            })
            .WithParsed((obj) =>
            {
                commandOptions = obj as ICommandOptions;
            });

        using ILoggerFactory loggerFactory = LoggerFactory.Create(builder =>
        {
            builder.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Information);
            builder.AddConsole();
            builder.AddEventSourceLogger();
        });

        ILogger logger = loggerFactory.CreateLogger("Startup");

        IHostBuilder hostBuilder = Host.CreateDefaultBuilder(args)
            .ConfigureAppConfiguration(configBuilder =>
            {
                // these should be set in the LaunchSettings.json or at runtime in Environment Variables
                string coreEnvironment = Environment.GetEnvironmentVariable("CS_ENVIRONMENT");
                string coreClusterCategory = Environment.GetEnvironmentVariable("CS_CATEGORY");

                configBuilder
                    .SetBasePath(AppContext.BaseDirectory);
                configBuilder
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
            });
        IHost host = hostBuilder.Build();

        if (commandOptions != null)
        {
            result = commandOptions.VerbRunner(host, logger);
        }
        return result;
    }
}