namespace sample.gateway.Tests;

using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Xunit;

public class ModelJsonTests
{
    [Fact]
    public void AllocationRoundTripPreservesServiceJsonAndAlertData()
    {
        const string json = """
            {
              "scope": {
                "tenantId": "11111111-1111-1111-1111-111111111111",
                "environmentId": "Default-11111111-1111-1111-1111-111111111111",
                "environmentGroupId": "22222222-2222-2222-2222-222222222222",
                "resourceId": "resource",
                "userId": "33333333-3333-3333-3333-333333333333"
              },
              "allocatedEntitlements": [{
                "entitlementId": "MCSMessages",
                "allocation": { "quantity": 0.0, "autoAllocated": 2.0, "unit": "Count" },
                "enforcementRules": [
                  { "ruleType": "TenantPool", "enabled": false },
                  { "ruleType": "Alert", "enabled": true,
                    "ruleData": { "unitType": "Percentage", "value": "80" } }
                ]
              }]
            }
            """;

        var settings = ModelJson.CreateSettings();
        var response = JsonConvert.DeserializeObject<AllocationResponseModel>(json, settings)!;
        var entitlement = Assert.Single(response.AllocatedEntitlements);
        var alert = Assert.IsType<AlertEnforcementRule>(entitlement.EnforcementRules[1]);
        Assert.Equal("80", alert.RuleData.Value);
        Assert.Equal(AllocationEnforcementRuleTypes.TenantPool, entitlement.EnforcementRules[0].RuleType);
        Assert.False(entitlement.EnforcementRules[0].Enabled);
        Assert.True(entitlement.IsEntitlementAllocated());

        var request = new AllocationPutRequestModel
        {
            Scope = response.Scope,
            AllocatedEntitlements = response.AllocatedEntitlements,
        };
        var actual = JObject.Parse(JsonConvert.SerializeObject(request, settings));

        Assert.True(JToken.DeepEquals(JObject.Parse(json), actual), actual.ToString());
    }

    [Fact]
    public void CurrencyAllocationSerializationPreservesStringEnumsAndOmitsNulls()
    {
        var request = new AllocationsByEnvironmentResponseModelV1
        {
            EnvironmentId = "environment",
            CurrencyAllocations =
            [
                new CurrencyAllocationResponseModelV1
                {
                    Allocated = 12,
                    CurrencyType = ExternalCurrencyType.MCSMessages,
                },
            ],
        };

        var actual = JObject.Parse(JsonConvert.SerializeObject(request, ModelJson.CreateSettings()));
        var expected = JObject.Parse("""
            {
              "environmentId": "environment",
              "currencyAllocations": [{ "allocated": 12, "currencyType": "MCSMessages" }]
            }
            """);

        Assert.True(JToken.DeepEquals(expected, actual), actual.ToString());
    }

    [Theory]
    [InlineData("""{"enabled":true}""")]
    [InlineData("""{"ruleType":"Invalid","enabled":true}""")]
    [InlineData("[]")]
    public void InvalidEnforcementRulesAreRejected(string json)
    {
        Assert.Throws<JsonSerializationException>(() =>
            JsonConvert.DeserializeObject<AllocationEnforcementRule>(json, ModelJson.CreateSettings()));
    }

    [Fact]
    public void IdentifierConvertersRemainAvailableForLocalIdentifierTypes()
    {
        var settings = ModelJson.CreateSettings();
        var tenantId = TenantId.Parse("11111111-1111-1111-1111-111111111111");

        var json = JsonConvert.SerializeObject(tenantId, settings);

        Assert.Equal("\"11111111-1111-1111-1111-111111111111\"", json);
        Assert.Equal(tenantId, JsonConvert.DeserializeObject<TenantId>(json, settings));
    }

    [Fact]
    public void LicenseEvaluationKeepsProductCategoryAsAStringWithoutSharedSettings()
    {
        var request = new LicenseEvaluationRequestModel<EmptyLicenseEvaluationContextInfo>
        {
            ProductCategory = ProductCategory.CopilotStudio,
        };

        var json = JObject.Parse(JsonConvert.SerializeObject(request));

        Assert.Equal("CopilotStudio", json[nameof(request.ProductCategory)]!.Value<string>());
    }

    [Theory]
    [InlineData(null, null, false, false)]
    [InlineData(0.0, 0.0, false, false)]
    [InlineData(1.0, null, false, true)]
    [InlineData(null, 1.0, false, true)]
    [InlineData(-1.0, 0.0, false, false)]
    [InlineData(0.0, 0.0, true, true)]
    public void AllocationExtensionUsesQuantitiesAndRulePresence(
        double? quantity, double? autoAllocated, bool hasRule, bool expected)
    {
        var entitlement = new EntitlementAllocationModel
        {
            Allocation = new AllocationModel { Quantity = quantity, AutoAllocated = autoAllocated },
            EnforcementRules = hasRule ? [new AllocationEnforcementRule { Enabled = false }] : [],
        };

        Assert.Equal(expected, entitlement.IsEntitlementAllocated());
    }

    [Fact]
    public void MissingAllocationAndRulesAreNotAllocated()
    {
        Assert.False(new EntitlementAllocationModel().IsEntitlementAllocated());
    }
}
