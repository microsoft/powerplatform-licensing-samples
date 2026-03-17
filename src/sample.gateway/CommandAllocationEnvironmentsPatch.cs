namespace sample.gateway;

using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Runtime.Versioning;
using System.Threading;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;
using Newtonsoft.Json.Serialization;

public class CommandAllocationEnvironmentsPatch : BaseCommand<CommandAllocationEnvironmentsPatchOptions>
{
    public CommandAllocationEnvironmentsPatch(
        CommandAllocationEnvironmentsPatchOptions opts,
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

            // BAP Environment Discovery
            Uri bapDomain = new Uri($"https://{_neptuneDiscovery.GetBapEndpoint()}"); // used to validate Nextlink contains relative base url
            Uri bapEnvironmentsUrl = new Uri(bapDomain, $"/providers/Microsoft.BusinessAppPlatform/scopes/admin/environments?api-version=2021-04-01&$top={Opts.PagingBy}&$select=name,properties.displayName,properties.createdTime,properties.tenantId,location");

            // Neptune PPAPI GW Tenant routing URL
            Uri gatewayTenantUri = new Uri($"https://{_neptuneDiscovery.GetGatewayEndpoint(Opts.TenantId)}");

            (bool flowControl, int value) = ProcessBapEnvironments(bapDomain, bapEnvironmentsUrl.ToString(), gatewayTenantUri, correlationId);
            if (!flowControl)
            {
                return value;
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
    private (bool flowControl, int value) ProcessBapEnvironments(Uri bapBaseUri, string bapEnvironmentsUrl, Uri gatewayTenantUri, Guid correlationId)
    {
        if (!IsSafeNextLink(bapBaseUri, bapEnvironmentsUrl))
        {
            TraceLogger.LogError("Unsafe nextLink detected, aborting operation.");
            return (flowControl: false, value: -1);
        }

        string gatewayResource = _neptuneDiscovery.GetTokenAudience();
        string tokenPrefix = _neptuneDiscovery.ClusterCategory.ToString();
        string tokenSuffix = "gateway";
        string gatewayAccessToken = OnAcquireUserToken(_clientId, gatewayResource, tokenPrefix, tokenSuffix).GetAwaiter().GetResult();
        if (string.IsNullOrWhiteSpace(gatewayAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for gateway.");
            return (flowControl: false, value: -1);
        }

        string bapResource = _neptuneDiscovery.GetBapAudience();
        string bapAccessToken = OnAcquireUserToken(_clientId, bapResource, tokenPrefix, "bap").GetAwaiter().GetResult();
        if (string.IsNullOrWhiteSpace(bapAccessToken))
        {
            TraceLogger.LogError("Failed to acquire token for BAP.");
            return (flowControl: false, value: -1);
        }

        string environmentsResponse = OnSendAsync(bapEnvironmentsUrl, Opts.TenantId, bapAccessToken, httpMethod: HttpMethod.Get, correlationId: correlationId, cancellationToken: CancellationToken.None);
        if (string.IsNullOrWhiteSpace(environmentsResponse))
        {
            TraceLogger.LogError("Failed to retrieve environments.");
            return (flowControl: false, value: -1);
        }

        BapPagedEntityResponse<BapEnvironmentScope> environments = JsonConvert.DeserializeObject<BapPagedEntityResponse<BapEnvironmentScope>>(environmentsResponse);
        foreach (BapEnvironmentScope environment in environments.Value)
        {
            TraceLogger.LogInformation("Environment: {EnvironmentName}, Display Name: {DisplayName}", environment.Name, environment.Properties.DisplayName);

            Uri allocationsUrl = new Uri(gatewayTenantUri, $"/licensing/allocations?$filter=environmentId eq '{environment.Name}' and EntitlementId in (MCSMessages,MCSSessions)&api-version=1");
            string allocationsResponse = OnSendAsync(allocationsUrl.ToString(), Opts.TenantId, gatewayAccessToken, HttpMethod.Get, correlationId: correlationId, cancellationToken: CancellationToken.None);

            bool HasChanges = false;

            AllocationPutRequestModel allocationPutRequestModel = new AllocationPutRequestModel
            {
                Scope = new ScopeModel
                {
                    TenantId = Opts.TenantId,
                    EnvironmentId = environment.Name,
                },
                AllocatedEntitlements = new List<EntitlementAllocationModel>(),
            };

            if (string.IsNullOrWhiteSpace(allocationsResponse))
            {
                HasChanges = true;
                TraceLogger.LogInformation("No allocations found for Environment: {EnvironmentName}", environment.Name);
                TraceLogger.LogInformation("Assuming no enforcement rules are set for this environment and defaulting to Tenant Pool enforcement for MCSMessages and MCSSessions entitlements.");
            }
            else
            {
                TraceLogger.LogInformation("Allocations found for Environment: {EnvironmentName}", environment.Name);

                // Can Put
                AllocationResponseModel allocationResponse = JsonConvert.DeserializeObject<AllocationResponseModel>(allocationsResponse);

                allocationPutRequestModel = new AllocationPutRequestModel
                {
                    Scope = new ScopeModel
                    {
                        TenantId = allocationResponse.Scope.TenantId,
                        EnvironmentId = allocationResponse.Scope.EnvironmentId,
                    },
                    AllocatedEntitlements = allocationResponse.AllocatedEntitlements ?? new List<EntitlementAllocationModel>(),
                };
            }

            // Always invoke both — short-circuit || would skip the call if HasChanges is already true
            bool mcsMessagesChanged = EnsureEntitlementEnforcement(allocationPutRequestModel, new EntitlementId("MCSMessages"), EnforcementRuleTypes.TenantPool);
            bool mcsSessionsChanged = EnsureEntitlementEnforcement(allocationPutRequestModel, new EntitlementId("MCSSessions"), EnforcementRuleTypes.TenantPool);
            HasChanges = HasChanges || mcsMessagesChanged || mcsSessionsChanged;

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

            if (!HasChanges)
            {
                TraceLogger.LogInformation("No Enforcement Rules to unset for Environment: {EnvironmentName}", environment.Name);
                continue; // No-Op
            }

            Uri allocationsPutUrl = new Uri(gatewayTenantUri, $"/licensing/allocations?api-version=1");

            if (Opts.WhatIf.HasValue) // typically null, if this --whatif is present in the pipeline skip it
            {
                /// Present the change could be made
                TraceLogger.LogInformation("WhatIf: Would PUT to {AllocationsPutUrl} with body: {AssertedChange}", allocationsPutUrl, assertedChange);
            }
            else
            {
                string putResponse = OnSendAsync(allocationsPutUrl.ToString(), Opts.TenantId, gatewayAccessToken, httpMethod: HttpMethod.Put, requestBody: assertedChange, correlationId: correlationId, cancellationToken: CancellationToken.None);
                if (!string.IsNullOrWhiteSpace(putResponse))
                {
                    TraceLogger.LogInformation("Successfully PUT allocations for Environment: {EnvironmentName}", environment.Name);
                    TraceLogger.LogInformation("Response: {PutResponse}", putResponse);
                }
            }
        }

        // Check if there are more environments to process
        if (environments.HasMore())
        {
            // Handle pagination if needed
            TraceLogger.LogInformation("More environments to process, next link: {NextLink}", environments.NextLink);
            return ProcessBapEnvironments(bapBaseUri, environments.NextLink, gatewayTenantUri, correlationId);
        }

        return (flowControl: true, value: default);
    }

    private bool EnsureEntitlementEnforcement(AllocationPutRequestModel allocationPutRequestModel, EntitlementId entitlementId, EnforcementRuleTypes enforcementRuleType)
    {
        EntitlementAllocationModel existingEntitlement = allocationPutRequestModel.AllocatedEntitlements.FirstOrDefault(fn => fn.EntitlementId == entitlementId);
        if (existingEntitlement == null)
        {
            // Create the entitlement with default rules
            EntitlementAllocationModel newEntitlement = new EntitlementAllocationModel
            {
                Allocation = new AllocationModel
                {
                    Quantity = 0, // Default quantity, can be adjusted later
                    AutoAllocated = 0 // Default auto allocation, can be adjusted later
                },
                EntitlementId = entitlementId,
                EnforcementRules = new List<EnforcementRule>
                {
                    new EnforcementRule
                    {
                        IsEnabled = Opts.Action == CommandAllocationEnvironmentsPatchOptionsAction.EnableDrawFromTenantPool,
                        Type = enforcementRuleType
                    }
                }
            };
            allocationPutRequestModel.AllocatedEntitlements.Add(newEntitlement);
            return true;
        }
        else
        {
            if (Opts.SkipExisting
                && existingEntitlement.EnforcementRules != null && existingEntitlement.EnforcementRules.Any(er => er.Type == enforcementRuleType))
            {
                TraceLogger.LogInformation("Skipping Environment: {EnvironmentId} for Entitlement: {EntitlementId} as it already has Enforcement Rule: {EnforcementRuleType}", allocationPutRequestModel.Scope.EnvironmentId, entitlementId, enforcementRuleType);
                return false; // Skip this entitlement as it already has the enforcement rule
            }

            // Update the entitlement with default rules
            if (existingEntitlement.EnforcementRules == null || existingEntitlement.EnforcementRules.Count == 0)
            {
                // backfill empty object with collection
                existingEntitlement.EnforcementRules = new List<EnforcementRule>();
            }

            EnforcementRule entitlementRule = existingEntitlement.EnforcementRules.FirstOrDefault(er => er.Type == enforcementRuleType);
            if (entitlementRule == null)
            {
                // If the entitlement exists but doesn't have the enforcement rule, add it
                existingEntitlement.EnforcementRules.Add(new EnforcementRule
                {
                    IsEnabled = Opts.Action == CommandAllocationEnvironmentsPatchOptionsAction.EnableDrawFromTenantPool,
                    Type = enforcementRuleType
                });
                return true;
            }
            else
            {
                // If the entitlement exists and has the enforcement rule
                if (entitlementRule.IsEnabled)
                {
                    // ensure it's set to false
                    if (Opts.Action == CommandAllocationEnvironmentsPatchOptionsAction.DisableDrawFromTenantPool)
                    {
                        entitlementRule.IsEnabled = false;
                        return true; // Change was made
                    }
                }
                else
                {
                    // ensure it's set to true
                    if (Opts.Action == CommandAllocationEnvironmentsPatchOptionsAction.EnableDrawFromTenantPool)
                    {
                        entitlementRule.IsEnabled = true;
                        return true; // Change was made
                    }
                }
            }
        }

        return false;
    }

}