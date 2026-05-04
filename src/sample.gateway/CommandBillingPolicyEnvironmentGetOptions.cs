namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

/// <summary>
/// Represents the options for the Billing Policy Get Environments check command
/// </summary>
/// <example>
/// CommandBillingPolicyEnvironmentGet --tenantId a1a2578a-8fd3-4595-bb18-7d17df8944b0 --billingPolicyId 4b62a25e-1c3d-e2bc-9270-307db9f15b00
/// </example>
public class CommandBillingPolicyEnvironmentGetOptions : CommandOptions
{
    public string BillingPolicyId { get; set; } = default(string);

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandBillingPolicyEnvironmentGet cmd = new(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }

    public static Command BuildCommandBillingPolicyEnvironmentGet(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();
        Option<string> billingPolicyId = new("--billingPolicyId")
        {
            Description = "Billing Policy Id to look up associated environments for.",
            Required = true
        };

        Command command = new("CommandBillingPolicyEnvironmentGet", "This will pull billing policies with environments.");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);
        command.Options.Add(billingPolicyId);

        command.SetAction(parseResult =>
        {
            var opts = new CommandBillingPolicyEnvironmentGetOptions
            {
                TenantId = parseResult.GetValue(tenantId),
                WhatIf = parseResult.GetValue(whatIf),
                BillingPolicyId = parseResult.GetValue(billingPolicyId),
            };
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            return opts.RunGenerateAndReturnExitCode(config, logger, host.Services);
        });

        return command;
    }
}
