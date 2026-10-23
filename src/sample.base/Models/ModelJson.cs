namespace sample.gateway.Models;

using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public static class ModelJson
{
    public static JsonSerializerSettings CreateSettings()
    {
        return new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new EnforcementRuleConverter(),
                new StringEnumConverter(),
                new ModelTypeConverter<EntitlementId>(),
                new ModelTypeConverter<EnvironmentGroupId>(),
                new ModelTypeConverter<EnvironmentId>(),
                new ModelTypeConverter<TenantId>(),
                new ModelTypeConverter<UserId>(),
            },
        };
    }
}
