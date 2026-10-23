namespace sample.gateway.Models;

using System.Linq;

public static class EntitlementAllocationModelExtensions
{
    public static bool IsEntitlementAllocated(this EntitlementAllocationModel entitlement)
    {
        ArgumentNullException.ThrowIfNull(entitlement);

        if (entitlement.Allocation != null
            && (entitlement.Allocation.Quantity > 0 || entitlement.Allocation.AutoAllocated > 0))
        {
            return true;
        }
        if (entitlement.EnforcementRules != null)
        {
            return entitlement.EnforcementRules.Any();
        }
        return false;
    }
}