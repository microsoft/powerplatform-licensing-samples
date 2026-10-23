namespace sample.gateway.Tests;

using System.CommandLine;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Xunit;

public class InteractiveCommandPromptTests
{
    [Fact]
    public void PromptsForAllocationOptionsAndRetriesInvalidValues()
    {
        using var host = new HostBuilder().Build();
        var command = CommandAllocationEnvironmentPatchOptions.BuildCommandAllocationEnvironmentPatch(host, NullLogger.Instance);
        var root = new RootCommand { command };
        ParseResult? captured = null;
        command.SetAction(result =>
        {
            captured = result;
            return 7;
        });
        using var input = new StringReader(string.Join("\n",
            "invalid", "0", "2", "1", "", "tenant",
            "", "yes", "true", "environment with spaces",
            "not-a-currency", "MCSMessages", "not-a-number", "12"));
        using var output = new StringWriter();

        Assert.Equal(7, InteractiveCommandPrompt.Invoke(root, [], input, output));

        Assert.NotNull(captured);
        Assert.Empty(captured.Errors);
        Assert.Equal("tenant", captured.GetValue<string>("--tenantId"));
        Assert.Null(captured.GetValue<string>("--clientId"));
        Assert.True(captured.GetValue<bool>("--whatif"));
        Assert.Equal("environment with spaces", captured.GetValue<string>("--environmentId"));
        Assert.Equal(ExternalCurrencyType.MCSMessages, captured.GetValue<ExternalCurrencyType>("--currencyType"));
        Assert.Equal(12, captured.GetValue<int>("--allocated"));
        Assert.Contains("Select a command from the list.", output.ToString());
        Assert.Contains("A value is required.", output.ToString());
        Assert.Contains("Choices:", output.ToString());
        Assert.Contains("Invalid value for --whatif: yes", output.ToString());
        Assert.Contains("not-a-currency", output.ToString());
        Assert.Contains("not-a-number", output.ToString());
    }

    [Fact]
    public void BlankOptionalValuesPreserveExistingDefaults()
    {
        using var host = new HostBuilder().Build();
        var command = CommandAllocationEnvironmentsPatchOptions.BuildCommandAllocationEnvironmentsPatch(host, NullLogger.Instance);
        var root = new RootCommand { command };
        ParseResult? captured = null;
        command.SetAction(result =>
        {
            captured = result;
            return 7;
        });
        using var input = new StringReader($"{command.Name}\ntenant\n\n\n\n\n\n");
        using var output = new StringWriter();

        Assert.Equal(7, InteractiveCommandPrompt.Invoke(root, [], input, output));

        Assert.NotNull(captured);
        Assert.Empty(captured.Errors);
        Assert.Equal("tenant", captured.GetValue<string>("--tenantId"));
        Assert.Null(captured.GetValue<string>("--clientId"));
        Assert.False(captured.GetValue<bool>("--whatif"));
        Assert.Equal(CommandAllocationEnvironmentsPatchOptionsAction.DisableDrawFromTenantPool,
            captured.GetValue<CommandAllocationEnvironmentsPatchOptionsAction>("--action"));
        Assert.Equal(50, captured.GetValue<int>("--pagingBy"));
        Assert.False(captured.GetValue<bool>("--skipExisting"));
        Assert.Contains("default: 50", output.ToString());
    }

    [Theory]
    [InlineData("")]
    [InlineData("q\n")]
    [InlineData("1\n")]
    [InlineData("1\nq\n")]
    [InlineData("1\ntenant\n")]
    [InlineData("1\ntenant\nQ\n")]
    public void CancellationOrEndOfInputDoesNotExecutePartialCommand(string answers)
    {
        var command = new Command("test");
        command.Options.Add(new Option<string>("--tenantId") { Required = true });
        command.Options.Add(new Option<string>("--clientId"));
        bool invoked = false;
        command.SetAction(_ => { invoked = true; });
        var root = new RootCommand { command };
        using var input = new StringReader(answers);
        using var output = new StringWriter();

        Assert.Equal(0, InteractiveCommandPrompt.Invoke(root, [], input, output));

        Assert.False(invoked);
        Assert.Contains("Command cancelled.", output.ToString());
    }

    [Fact]
    public void ExplicitArgumentsBypassPromptsAndPreserveExitCode()
    {
        var command = new Command("test");
        var tenant = new Option<string>("--tenantId") { Required = true };
        command.Options.Add(tenant);
        command.SetAction(result => result.GetValue(tenant) == "tenant" ? 7 : 1);
        var root = new RootCommand { command };
        using var input = new StringReader("unused input");
        using var output = new StringWriter();

        Assert.Equal(7, InteractiveCommandPrompt.Invoke(root, ["test", "--tenantId", "tenant"], input, output));

        Assert.Equal("", output.ToString());
        Assert.Equal("unused input", input.ReadToEnd());
    }
}
