namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

public class CommandAllocationEnvironmentGetOptions : CommandOptions
{
    public string EnvironmentId { get; set; }

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationEnvironmentGet cmd = new CommandAllocationEnvironmentGet(this, configuration, logger, serviceProvider);
        int result = cmd.Run();
        return result;
    }

    public static Command BuildCommandAllocationEnvironmentGet(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();
        Option<string> environmentId = EnvironmentIdOption();

        Command command = new("CommandAllocationEnvironmentGet", "This will pull allocation documents for the environment.");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);
        command.Options.Add(environmentId);

        command.SetAction(parseResult =>
        {
            var opts = new CommandAllocationEnvironmentGetOptions
            {
                TenantId = parseResult.GetValue(tenantId),
                WhatIf = parseResult.GetValue(whatIf),
                EnvironmentId = parseResult.GetValue(environmentId),
            };
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            return opts.RunGenerateAndReturnExitCode(config, logger, host.Services);
        });

        return command;
    }
}
