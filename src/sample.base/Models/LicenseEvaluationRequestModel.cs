namespace sample.gateway.Models;

using System.Collections.Generic;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

public class LicenseEvaluationRequestModel<T> where T : LicenseEvaluationContextInfo, new()
{
    public List<LicenseEvaluationType> EvaluationTypes { get; set; }

    [JsonConverter(typeof(StringEnumConverter))]
    public ProductCategory ProductCategory { get; set; }

    public HashSet<string> Entitlements { get; set; }

    public T ContextInformation { get; set; }
}