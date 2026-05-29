namespace sample.gateway;

using System.CommandLine;
using Microsoft.Extensions.Hosting;

public class CommandAllocationEnvironmentsPatchOptions : CommandOptions
{
    public CommandAllocationEnvironmentsPatchOptionsAction Action { get; set; } = CommandAllocationEnvironmentsPatchOptionsAction.DisableDrawFromTenantPool;

    public int PagingBy { get; set; } = 50;

    public bool SkipExisting { get; set; } = false;

    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationEnvironmentsPatch cmd = new(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }

    public static Command BuildCommandAllocationEnvironmentsPatch(IHost host, ILogger logger)
    {
        Option<string> tenantId = TenantIdOption();
        Option<bool> whatIf = WhatIfOption();
        Option<CommandAllocationEnvironmentsPatchOptionsAction> action = new("--action")
        {
            Description = "Action to perform.",
            DefaultValueFactory = _ => CommandAllocationEnvironmentsPatchOptionsAction.DisableDrawFromTenantPool
        };
        Option<int> pagingBy = new("--pagingBy")
        {
            Description = "Number of items to page by.",
            DefaultValueFactory = _ => 50
        };
        Option<bool> skipExisting = new("--skipExisting")
        {
            Description = "Skip allocation documents if rule is present."
        };

        Command command = new("CommandAllocationEnvironmentsPatch");
        command.Options.Add(tenantId);
        command.Options.Add(whatIf);
        command.Options.Add(action);
        command.Options.Add(pagingBy);
        command.Options.Add(skipExisting);

        command.SetAction(parseResult =>
        {
            var opts = new CommandAllocationEnvironmentsPatchOptions
            {
                TenantId = parseResult.GetValue(tenantId),
                WhatIf = parseResult.GetValue(whatIf),
                Action = parseResult.GetValue(action),
                PagingBy = parseResult.GetValue(pagingBy),
                SkipExisting = parseResult.GetValue(skipExisting),
            };
            IConfiguration config = host.Services.GetRequiredService<IConfiguration>();
            return opts.RunGenerateAndReturnExitCode(config, logger, host.Services);
        });

        return command;
    }
}
