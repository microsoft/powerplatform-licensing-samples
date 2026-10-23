namespace sample.gateway;

using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;

public class CommandAllocationEnvironmentGet : BaseCommand<CommandAllocationEnvironmentGetOptions>
{
    public CommandAllocationEnvironmentGet(
        CommandAllocationEnvironmentGetOptions opts,
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider) : base(opts, configuration, logger, serviceProvider)
    {
    }

    // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
    [SupportedOSPlatform("windows")]
    public override int OnRun()
    {
        try
        {
            /// You need to be a Tenant Admin or an Environment Admin
            /// 
            // Neptune PPAPI GW Tenant routing URL
            string tenantUrl = _neptuneDiscovery.GetGatewayEndpoint(Opts.TenantId);
            Uri gatewayTenantUri = new Uri($"https://{tenantUrl}");

            (bool flowControl, string value) = GetEnvironmentAllocation(gatewayTenantUri, Opts.TenantId, Opts.EnvironmentId);
            if (flowControl)
            {
                AllocationsByEnvironmentResponseModelV1 results = Newtonsoft.Json.JsonConvert.DeserializeObject<AllocationsByEnvironmentResponseModelV1>(value, ModelJson.CreateSettings());

                return 0;
            }

            return -1;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(ex.Message);
            return -1;
        }
    }

    // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
    [SupportedOSPlatform("windows")]
    private (bool flowControl, string value) GetEnvironmentAllocation(Uri gatewayTenantUri, TenantId tenantId, EnvironmentId environmentId)
    {
        string gatewayResource = _neptuneDiscovery.GetTokenAudience();
        string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
        string tokenSuffix = "gateway";

        string gatewayAccessToken = OnAcquireUserToken(_clientId, gatewayResource, tokenPrefix, tokenSuffix).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(gatewayAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for gateway.");
            return (flowControl: false, value: string.Empty);
        }

        bool responseOk = false;
        Uri allocationsUrl = new Uri(gatewayTenantUri, $"/licensing/environments/{environmentId}/allocations?api-version=2022-03-01-preview");
        string gatewayResponse = OnSendAsync(allocationsUrl.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Get, correlationId: Guid.NewGuid(), cancellationToken: CancellationToken.None);
        if (!string.IsNullOrWhiteSpace(gatewayResponse))
        {
            TraceLogger.LogInformation($"Succeeded {allocationsUrl}.");
            TraceLogger.LogInformation($"Allocations for {environmentId}: {gatewayResponse}");
            responseOk = true;
        }

        return (flowControl: responseOk, value: gatewayResponse);
    }

}