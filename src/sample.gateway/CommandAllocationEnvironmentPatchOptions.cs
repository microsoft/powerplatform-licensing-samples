namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

public class CommandAllocationEnvironmentPatchOptions : CommandOptions
{
    public string EnvironmentId { get; set; }

    public int Allocated { get; set; } = 0;

    public ExternalCurrencyType CurrencyType { get; set; }

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationEnvironmentPatch cmd = new CommandAllocationEnvironmentPatch(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }

    public static Command BuildCommandAllocationEnvironmentPatch(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();
        Option<string> environmentId = EnvironmentIdOption();
        Option<ExternalCurrencyType> currencyType = new("--currencyType")
        {
            Description = "Currency to modify.",
            Required = true
        };
        Option<int> allocated = new("--allocated")
        {
            Description = "Numeric value for allocation.",
            DefaultValueFactory = _ => 0
        };

        Command command = new("CommandAllocationEnvironmentPatch");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);
        command.Options.Add(environmentId);
        command.Options.Add(currencyType);
        command.Options.Add(allocated);

        command.SetAction(parseResult =>
        {
            var opts = new CommandAllocationEnvironmentPatchOptions
            {
                TenantId = parseResult.GetValue(tenantId),
                WhatIf = parseResult.GetValue(whatIf),
                EnvironmentId = parseResult.GetValue(environmentId),
                CurrencyType = parseResult.GetValue(currencyType),
                Allocated = parseResult.GetValue(allocated),
            };
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            return opts.RunGenerateAndReturnExitCode(config, logger, host.Services);
        });

        return command;
    }
}
