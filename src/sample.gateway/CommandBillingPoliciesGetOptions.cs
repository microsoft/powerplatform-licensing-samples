namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Represents the options for the Billing Policy Get List check command
/// </summary>
/// <example>
/// CommandBillingPoliciesGet --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0
/// </example>
public class CommandBillingPoliciesGetOptions : CommandOptions
{
    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandBillingPoliciesGet cmd = new(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }

    public static Command BuildCommandBillingPoliciesGet(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();

        Command command = new("CommandBillingPoliciesGet", "Represents the options for the Billing Policy Get List check command.");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);

        command.SetAction(parseResult =>
        {
            var opts = new CommandBillingPoliciesGetOptions
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
