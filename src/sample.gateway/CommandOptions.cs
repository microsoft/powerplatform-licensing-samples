namespace sample.gateway;

/// <summary>
/// Common options for all commands
/// </summary>
public class CommandOptions : ICommandOptions
{
    [Option("whatif", Required = false, HelpText = "provides for what if scenarios, present changes before asserting them.")]
    public bool? WhatIf { get; set; }

    [Option("tenantId", Required = true, SetName = "AllParameterSets", HelpText = "Tenant Id for downstream user tenant.")]
    public string TenantId { get; set; } = default(string);
}