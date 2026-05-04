namespace sample.gateway;

[Verb("CommandAllocationEnvironmentPatch")]
public class CommandAllocationEnvironmentPatchOptions : CommandOptions
{
    [Option("environmentId", Required = true, SetName = "AllParameterSets", HelpText = "Environment Id for which token will be issued")]
    public string EnvironmentId { get; set; }

    [Option("allocated", Required = false, SetName = "AllParameterSets", HelpText = "Numeric value for allocation")]
    public int Allocated { get; set; } = 0;

    [Option("currencyType", Required = true, SetName = "AllParameterSets", HelpText = "Currency to modify.")]
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
}