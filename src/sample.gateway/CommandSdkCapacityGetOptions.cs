namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

public class CommandSdkCapacityGetOptions : CommandOptions
{
    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandSdkCapacityGet cmd = new(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }

    public static Command BuildCommandSdkCapacityGet(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<string> clientId = ClientIdOption();
        Option<bool> whatIf = WhatIfOption();

        Command command = new("CommandSdkCapacityGet");
        command.Options.Add(tenantId);
        command.Options.Add(clientId);
        command.Options.Add(whatIf);

        command.SetAction(parseResult =>
        {
            var opts = new CommandSdkCapacityGetOptions
            {
                TenantId = parseResult.GetValue(tenantId),
                ClientId = parseResult.GetValue(clientId),
                WhatIf = parseResult.GetValue(whatIf),
            };
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            return opts.RunGenerateAndReturnExitCode(config, logger, host.Services);
        });

        return command;
    }
}
