namespace sample.gateway;

using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;

public class CommandBillingPolicyEnvironmentGet : BaseCommand<CommandBillingPolicyEnvironmentGetOptions>
{
    public CommandBillingPolicyEnvironmentGet(
        CommandBillingPolicyEnvironmentGetOptions opts,
        IConfiguration configuration,
        ILogger logger,
        IServiceProvider serviceProvider) : base(opts, configuration, logger, serviceProvider)
    {
    }

    // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
    [SupportedOSPlatform("windows")]
    public override int OnRun()
    {
        string gatewayUrl = _neptuneDiscovery.GetGatewayEndpoint(Opts.TenantId);
        string gatewayResource = _neptuneDiscovery.GetTokenAudience();

        // API Endpoint
        string clusterurl = $"https://{gatewayUrl}/licensing/billingPolicies/{Opts.BillingPolicyId}/environments?api-version=1";

        TraceLogger.LogInformation($"Gateway URL: {clusterurl}");
        TraceLogger.LogInformation($"Gateway Audience: {gatewayResource}");

        if (!Opts.WhatIf)
        {
            GetEnvironmentBillingPolicies(clusterurl, Opts.TenantId, gatewayResource, CancellationToken.None);
        }

        return 0;
    }

    // Add the SupportedOSPlatform attribute to the method where TokenStorage.SaveToken is called
    [SupportedOSPlatform("windows")]
    private (bool flowControl, string value) GetEnvironmentBillingPolicies(string url, string tenantId, string gatewayResource, CancellationToken cancellationToken = default)
    {
        string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
        string tokenSuffix = "gateway";

        string gatewayAccessToken = OnAcquireUserToken(_clientId, gatewayResource, tokenPrefix, tokenSuffix, cancellationToken).GetAwaiter().GetResult();

        if (string.IsNullOrWhiteSpace(gatewayAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for gateway.");
            return (flowControl: false, value: string.Empty);
        }

        bool responseOk = false;
        string gatewayResponse = OnSendAsync(url.ToString(), tenantId, gatewayAccessToken, httpMethod: HttpMethod.Get, correlationId: Guid.NewGuid(), cancellationToken: cancellationToken);
        if (!string.IsNullOrWhiteSpace(gatewayResponse))
        {
            TraceLogger.LogInformation($"Succeeded {url}.");
            TraceLogger.LogInformation($"billing environments for {Opts.TenantId}: {gatewayResponse}");
            responseOk = true;
        }

        return (flowControl: responseOk, value: gatewayResponse);
    }

}