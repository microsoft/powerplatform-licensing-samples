namespace sample.gateway;

using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using Newtonsoft.Json;

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

            (HttpStatusCode? statusCode, string value) = GetEnvironmentAllocation(gatewayTenantUri, Opts.TenantId, Opts.EnvironmentId);
            if (statusCode == HttpStatusCode.NotFound)
            {
                TraceLogger.LogInformation("No allocation document found for Environment: {EnvironmentId}. Creating an allocation for currency type: {CurrencyType}.", Opts.EnvironmentId, Opts.CurrencyType);
            }
            else if (statusCode.HasValue && (int)statusCode.Value >= 200 && (int)statusCode.Value < 300
                && !string.IsNullOrWhiteSpace(value))
            {
                var results = JsonConvert.DeserializeObject<AllocationsByEnvironmentResponseModelV1>(value, ModelJson.CreateSettings());
                if (results == null)
                {
                    TraceLogger.LogError("The allocation response did not contain a document for Environment: {EnvironmentId}.", Opts.EnvironmentId);
                    return -1;
                }
                if (results.CurrencyAllocations?.Any(ca => ca.CurrencyType == Opts.CurrencyType && ca.Allocated == Opts.Allocated) == true)
                {
                    TraceLogger.LogError($"No allocations need to be modified for currency type: {Opts.CurrencyType}");
                    return -1;
                }
            }
            else
            {
                TraceLogger.LogError("Failed to retrieve allocations for Environment: {EnvironmentId}. Status code: {StatusCode}.", Opts.EnvironmentId, statusCode);
                return -1;
            }

            (bool patchFlowControl, string patchValue) = PatchEnvironmentAllocation(gatewayTenantUri, Opts.TenantId, Opts.EnvironmentId, Opts.CurrencyType, Opts.Allocated, correlationId);
            return patchFlowControl ? 0 : -1;
        }
        catch (Exception ex)
        {
            TraceLogger.LogError(ex.Message);
            return -1;
        }
    }

    // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
    [SupportedOSPlatform("windows")]
    private (HttpStatusCode? statusCode, string value) GetEnvironmentAllocation(Uri gatewayTenantUri, TenantId tenantId, EnvironmentId environmentId)
    {
        string gatewayResource = _neptuneDiscovery.GetTokenAudience();
        string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
        string tokenSuffix = "gateway";

        string gatewayAccessToken = OnAcquireUserToken(_clientId, gatewayResource, tokenPrefix, tokenSuffix).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(gatewayAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for gateway.");
            return (statusCode: null, value: string.Empty);
        }

        Uri allocationsUrl = new Uri(gatewayTenantUri, $"/licensing/environments/{environmentId}/allocations?api-version=2022-03-01-preview");
        var response = OnSendWithStatusAsync(allocationsUrl.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Get, correlationId: Guid.NewGuid(), cancellationToken: CancellationToken.None);
        if (response.statusCode.HasValue && (int)response.statusCode.Value >= 200 && (int)response.statusCode.Value < 300)
        {
            TraceLogger.LogInformation($"Succeeded {allocationsUrl}.");
            TraceLogger.LogInformation($"Allocations for {environmentId}: {response.value}");
        }

        return response;
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

        string assertedChange = JsonConvert.SerializeObject(allocationPutRequestModel, ModelJson.CreateSettings());

        bool responseOk = false;
        Uri allocationsPutUrl = new Uri(gatewayTenantUri, $"/licensing/environments/{environmentId}/allocations?api-version=2022-03-01-preview");

        if (Opts.WhatIf)
        {
            /// Present the change could be made
            TraceLogger.LogInformation("WhatIf: Would PATCH to {AllocationsPutUrl} with body: {AssertedChange}", allocationsPutUrl, assertedChange);
            responseOk = true;
        }
        else
        {
            var response = OnSendWithStatusAsync(allocationsPutUrl.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Patch, requestBody: assertedChange, correlationId: correlationId, cancellationToken: CancellationToken.None);
            if (response.statusCode.HasValue && (int)response.statusCode.Value >= 200 && (int)response.statusCode.Value < 300)
            {
                responseOk = true;
                TraceLogger.LogInformation("Successfully PATCHed allocations for Environment: {environmentId}", environmentId);
                TraceLogger.LogInformation("Response: {PutResponse}", response.value);
            }
            else
            {
                TraceLogger.LogError("Failed to PATCH allocations for Environment: {EnvironmentId}. Status code: {StatusCode}.", environmentId, response.statusCode);
            }
        }

        return (flowControl: responseOk, value: default);
    }

}