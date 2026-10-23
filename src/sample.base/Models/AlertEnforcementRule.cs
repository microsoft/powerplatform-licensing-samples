namespace sample.gateway.Models;

using Newtonsoft.Json;

public class AlertEnforcementRule : AllocationEnforcementRule
{
    [JsonProperty(PropertyName = "ruleData")]
    public AlertData RuleData { get; set; }
}