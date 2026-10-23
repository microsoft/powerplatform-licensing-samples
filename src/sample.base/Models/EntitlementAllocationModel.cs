namespace sample.gateway.Models;

using System.Linq;

public partial class EntitlementAllocationModelPartial : EntitlementAllocationModel
{

    public bool IsEntitlementAllocated()
    {
        if (Allocation != null
            && (Allocation.Quantity > 0 || Allocation.AutoAllocated > 0))
        {
            return true;
        }
        if (EnforcementRules != null)
        {
            return EnforcementRules.Any();
        }
        return false;
    }
}