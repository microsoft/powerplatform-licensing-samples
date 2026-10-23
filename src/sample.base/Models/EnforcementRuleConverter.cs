namespace sample.gateway.Models;

using Newtonsoft.Json;
using System.Globalization;
using Newtonsoft.Json.Linq;

public class EnforcementRuleConverter : Newtonsoft.Json.JsonConverter
{
    public override bool CanWrite
    {
        get { return false; }
    }

    public override bool CanConvert(Type objectType)
    {
        return typeof(AllocationEnforcementRule).IsAssignableFrom(objectType);
    }

    public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
    {
        throw new NotImplementedException();
    }

    /// <summary>Reads the JSON representation of the object.</summary>
    /// <param name="reader">
    ///     The <see cref="Newtonsoft.Json.JsonReader" /> to read from.
    /// </param>
    /// <param name="objectType">Type of the object.</param>
    /// <param name="existingValue">The existing value of object being read.</param>
    /// <param name="serializer">The calling serializer.</param>
    /// <returns>The object value.</returns>
    public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
    {
        if (reader.TokenType != JsonToken.StartObject)
        {
            throw new JsonSerializationException("Expected start of object in the JSON but found: " + reader.TokenType);
        }

        JObject value = JObject.Load(reader);

        // Create target object based on JObject
        AllocationEnforcementRule target = this.CreateObject(objectType, value, serializer);

        serializer.Populate(value.CreateReader(), target);

        return target;
    }

    /// <summary>
    /// Create an instance of a subclass of the target type based on the JObject contents.
    /// </summary>
    /// <param name="objectType">Target type.</param>
    /// <param name="jobject">The JSON representing the target object.</param>
    /// <param name="serializer">Serializer being used for deserialization.</param>
    /// <returns>The instance of the subclass to populate.</returns>
    private AllocationEnforcementRule CreateObject(Type objectType, JObject jobject, JsonSerializer serializer)
    {
        JToken token;

        if (!jobject.TryGetValue("ruleType", out token))
        {
            throw new JsonSerializationException(
                string.Format(
                    CultureInfo.InvariantCulture,
                    "Could not deserialize the Enforcement Rule because the {0} property was not present.",
                    "ruleType"));
        }

        AllocationEnforcementRuleTypes enforcementRuleType = token.ToObject<AllocationEnforcementRuleTypes>(serializer);

        switch (enforcementRuleType)
        {
            case AllocationEnforcementRuleTypes.Alert:
                return new AlertEnforcementRule();
            default:
                return new AllocationEnforcementRule();
        }
    }
}