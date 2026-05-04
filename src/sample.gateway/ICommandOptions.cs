namespace sample.gateway;

public interface ICommandOptions
{
    bool WhatIf { get; set; }

    /// <summary>
    /// Required TenantId for authentication purposes.
    /// </summary>
    string TenantId { get; set; }
}
