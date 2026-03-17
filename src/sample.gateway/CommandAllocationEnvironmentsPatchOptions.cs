namespace sample.gateway;

[Verb("CommandAllocationEnvironmentsPatch")]
public class CommandAllocationEnvironmentsPatchOptions : CommandOptions
{
    [Option(shortName: 'a', "action", Required = false, SetName = "AllParameterSets", HelpText = "Action to perform")]
    public CommandAllocationEnvironmentsPatchOptionsAction Action { get; set; } = CommandAllocationEnvironmentsPatchOptionsAction.DisableDrawFromTenantPool;

    [Option(shortName: 'p', "pagingBy", Required = false, SetName = "AllParameterSets", HelpText = "Number of items to page by")]
    public int PagingBy { get; set; } = 50;

    [Option(shortName: 's', "skipExisting", Required = false, SetName = "AllParameterSets", HelpText = "Skip allocation documents if rule is present.")]
    public bool SkipExisting { get; set; } = false;


    public int RunGenerateAndReturnExitCode(
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider)
    {
        CommandAllocationEnvironmentsPatch cmd = new CommandAllocationEnvironmentsPatch(this, configuration, logger, serviceProvider);
        var result = cmd.Run();
        return result;
    }
}