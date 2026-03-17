namespace sample.gateway.Models;

using Newtonsoft.Json;

public class BapThrottleError
{
    [JsonProperty(PropertyName = "code")]
    public string Code { get; set; }

    [JsonProperty(PropertyName = "message")]
    public string Message { get; set; }

    [JsonProperty(PropertyName = "defaultUrlType")]
    public string DefaultUrlType { get; set; }
}
