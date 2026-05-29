namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Represents the options for the Entitlement check command, which uses the gateway cluster or island tenant/environment cluster routes
/// </summary>
/// <example>
///
/// Check Environment Entitlement using the Environment Route
/// CommandEnvironmentBillingGet --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0 --environmentId 4b62a25e-1c3d-e2bc-9270-307db9f15b00
///
/// </example>
public class CommandEnvironmentBillingGetOptions : CommandOptions
{
    public string EnvironmentId { get; set; } = default(string);

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandEnvironmentBillingGet cmd = new(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }


    public static Command BuildCommandEnvironmentBillingGet(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();
        Option<string> environmentId = new("--environmentId")
        {
            Description = "Environment Id for which the billing policy will be retrieved.",
            Required = true
        };

        Command command = new("CommandEnvironmentBillingGet");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);
        command.Options.Add(environmentId);

        command.SetAction(parseResult =>
        {
            var opts = new CommandEnvironmentBillingGetOptions
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
