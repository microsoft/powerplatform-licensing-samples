namespace sample.gateway.Models;

using Newtonsoft.Json;

public class BapThrottleResponse
{
    public BapThrottleResponse()
    {
        Error = new BapThrottleError();
    }

    [JsonProperty(PropertyName = "error")]
    public BapThrottleError Error { get; set; }
}
