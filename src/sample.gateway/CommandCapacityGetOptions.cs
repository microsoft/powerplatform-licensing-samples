namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

public class CommandCapacityGetOptions : CommandOptions
{
    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandCapacityGet cmd = new(this, configuration, logger, serviceProvider);
        int result = cmd.Run();
        return result;
    }

    public static Command BuildCommandCapacityGet(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();

        Command command = new("CommandCapacityGet", "Get currency reports for the tenant.");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);

        command.SetAction(parseResult =>
        {
            var opts = new CommandCapacityGetOptions
            {
                TenantId = parseResult.GetValue(tenantId),
                WhatIf = parseResult.GetValue(whatIf),
            };
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            return opts.RunGenerateAndReturnExitCode(config, logger, host.Services);
        });

        return command;
    }
}
