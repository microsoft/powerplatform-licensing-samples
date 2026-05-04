namespace sample.gateway;

using System.CommandLine;

/// <summary>
/// Common options for all commands
/// </summary>
public class CommandOptions : ICommandOptions
{
    public bool WhatIf { get; set; }

    public string TenantId { get; set; } = default(string);

    internal static Option<string> EnvironmentIdOption() => new("--environmentId")
    {
        Description = "Environment Id for which the allocation will be retrieved.",
        Required = true
    };

    internal static Option<string> TenantIdOption() => new("--tenantId")
    {
        Description = "Tenant Id for downstream user tenant.",
        Required = true
    };

    internal static Option<bool> WhatIfOption() => new("--whatif")
    {
        Description = "Provides for what-if scenarios, present changes before asserting them."
    };
}
