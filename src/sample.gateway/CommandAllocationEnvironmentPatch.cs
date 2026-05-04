namespace sample.gateway;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public class CommandAllocationEnvironmentPatch : BaseCommand<CommandAllocationEnvironmentPatchOptions>
{
    public CommandAllocationEnvironmentPatch(
        CommandAllocationEnvironmentPatchOptions opts,
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
            Guid correlationId = Guid.NewGuid(); // For tracing purposes, associate all calls in this run with this correlation ID

            /// You need to be a Tenant Admin or an Environment Admin
            /// 
            // Neptune PPAPI GW Tenant routing URL
            string tenantUrl = _neptuneDiscovery.GetGatewayEndpoint(Opts.TenantId);
            Uri gatewayTenantUri = new Uri($"https://{tenantUrl}");

            (bool flowControl, string value) = GetEnvironmentAllocation(gatewayTenantUri, Opts.TenantId, Opts.EnvironmentId);
            if (flowControl)
            {
                AllocationsByEnvironmentResponseModelV1 results = Newtonsoft.Json.JsonConvert.DeserializeObject<AllocationsByEnvironmentResponseModelV1>(value);
                if (results.CurrencyAllocations.Any(ca => ca.CurrencyType == Opts.CurrencyType && ca.Allocated == Opts.Allocated))
                {
                    TraceLogger.LogError($"No allocations need to be modified for currency type: {Opts.CurrencyType}");
                    return -1;
                }
                else
                {
                    (bool patchFlowControl, string patchValue) = PatchEnvironmentAllocation(gatewayTenantUri, Opts.TenantId, Opts.EnvironmentId, Opts.CurrencyType, Opts.Allocated, correlationId);
                    if (!patchFlowControl)
                    {
                        return -1;
                    }
                }
                return 0;
            }
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(ex.Message);
            return -1;
        }
        return 0;
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

    // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
    [SupportedOSPlatform("windows")]
    private (bool flowControl, string value) PatchEnvironmentAllocation(Uri gatewayTenantUri, TenantId tenantId, EnvironmentId environmentId, ExternalCurrencyType currencyType, int allocate, Guid correlationId)
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

        AllocationsByEnvironmentResponseModelV1 allocationPutRequestModel = new AllocationsByEnvironmentResponseModelV1
        {
            EnvironmentId = environmentId,
            CurrencyAllocations = new List<CurrencyAllocationResponseModelV1>()
            {
                new CurrencyAllocationResponseModelV1()
                {
                    Allocated = allocate,
                    CurrencyType = currencyType
                }
            }
        };

        string assertedChange = JsonConvert.SerializeObject(allocationPutRequestModel, new JsonSerializerSettings
        {
            NullValueHandling = NullValueHandling.Ignore,
            Formatting = Formatting.Indented,
            ContractResolver = new CamelCasePropertyNamesContractResolver(),
            Converters = new List<JsonConverter>
            {
                new StringEnumConverter(),
                new ModelTypeConverter<EntitlementId>(),
                new ModelTypeConverter<EnvironmentGroupId>(),
                new ModelTypeConverter<EnvironmentId>(),
                new ModelTypeConverter<TenantId>(),
                new ModelTypeConverter<UserId>(),
            },
        });

        bool responseOk = false;
        Uri allocationsPutUrl = new Uri(gatewayTenantUri, $"/licensing/environments/{environmentId}/allocations?api-version=2022-03-01-preview");

        if (Opts.WhatIf)
        {
            /// Present the change could be made
            TraceLogger.LogInformation("WhatIf: Would PUT to {AllocationsPutUrl} with body: {AssertedChange}", allocationsPutUrl, assertedChange);
        }
        else
        {
            string putResponse = OnSendAsync(allocationsPutUrl.ToString(), Opts.TenantId, gatewayAccessToken, httpMethod: HttpMethod.Patch, requestBody: assertedChange, correlationId: correlationId, cancellationToken: CancellationToken.None);
            if (!string.IsNullOrWhiteSpace(putResponse))
            {
                responseOk = true;
                TraceLogger.LogInformation("Successfully PUT allocations for Environment: {environmentId}", environmentId);
                TraceLogger.LogInformation("Response: {PutResponse}", putResponse);
            }
        }

        return (flowControl: responseOk, value: default);
    }

}