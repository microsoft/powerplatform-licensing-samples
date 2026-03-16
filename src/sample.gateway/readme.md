# SampleGateway

```
// This sample code is provided AS-IS, without warranties or conditions of any kind, express or implied, including without limitation any implied warranties or conditions of title, fitness for a particular purpose, merchantability, or non-infringement.
//
// You may use this project to understand how to use the Power Platform Licensing APIs.
//
// The sample commands are provided for demonstration purposes only.
// We do not guarantee the sample commands work as you expect.
// You may need to modify the sample commands to fit your specific needs.
// Some commands support a WhatIf parameter to preview the changes without applying them.
//
// While these are demonstration commands, they will modify your environment.
// Use them at your own risk.
//
```

## Package References

The project should restore from publically available nuget feeds.  

## .NET Version

This project targets .NET 10.0 and C# 14.0. Ensure your development environment is set up accordingly.

## Caution

This sample code is provided "AS-IS", without warranties or conditions of any kind, express or implied. Use it at your own risk.

## Launch Profiles

The project includes launch profiles for different configurations. Use the Run and Debug pane (Ctrl+Shift+D) in Visual Studio and use the desired configuration.

## Available Commands

The commands below demonstrate how to interact with the Power Platform Licensing APIs.  To run these commands, ensure you have the necessary permissions (e.g., Tenant Admin) and that your environment is set up correctly.

To run these commands you have a few options:
1. Use the integrated terminal in Visual Studio or Visual Studio Code.
1. Use any other terminal of your choice.

Running these commands will launch an authentication flow to acquire a user token for the specified tenant.  
Ensure you have the necessary permissions to execute these commands.  
The command uses a launch profile named "PlatformProd" which is pre-configured for production scenarios.

The examples below use a sample tenant id and environment id.  You should change these to your tenant id and environment id.  To find these details you can use PPAC (Power Platform Admin Center) or other tools.

Sample commands executed in the terminal `developer console`:

```terminal

#REM Get the Billing Policies
> dotnet run --launch-profile "PlatformProd" "CommandBillingPoliciesGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --project src/sample.gateway

#REM Get the environments associatd with the Billing Policy
> dotnet run --launch-profile "PlatformProd" "CommandBillingPolicyEnvironmentGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --billingPolicyId 1c590f8e-92ba-4f4b-84b4-2d048daebe34 --project src/sample.gateway

REM Get the Environment Billing Policy details
> dotnet run --launch-profile "PlatformProd" "CommandEnvironmentBillingGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02 --project src/sample.gateway

```

### CommandCapacityGet

This command will return the tenant capacity for the specified tenant.

https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/tenant-capacity-details/get-tenant-capacity-details

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.

### CommandAllocationEnvironmentGet

This command will enumerate the environments for which you have access and return the licensing allocation for the specified tenant.

https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/currency-allocation/get-currency-allocation-by-environment

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.
  - environmentId: (Required) The environment id for which the operation will execute.  If not specified, all environments will be returned.

### CommandAllocationEnvironmentPatch

This command will patch the allocation currency for the specified environment for which you have access.  Please note, this is a Legacy API and is only supported for environments created before Tenant Pool licensing was introduced.
*This will apply the changes.*

https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/currency-allocation/patch-currency-allocation-by-environment

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.
  - environmentId: (Required) The environment id for which the operation will execute.  If not specified, all environments will be returned.
  - Allocated: (Required) int value to adjust the allocated amount.
  - CurrencyType: (Required) Enum value to set the currency type (AI,PowerAutomatePerProcess,ProcessMiningDataStorage).
  - WhatIf: (Optional) this parameter will simulate the changes without actually applying them.

### CommandAllocationEnvironmentsPatch

This command will enumerate the environments for which you have access and enable or disable draw from Tenant Pool licensing for each environment.
*This will apply the changes.*

<undocumented endpoint>

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.
  - Action: (Optional) Enum value to enable (EnableDrawFromTenantPool) or disable (DisableDrawFromTenantPool) draw from Tenant Pool licensing.
  - WhatIf: (Optional) this parameter will simulate the changes without actually applying them.

### CommandEnvironmentBillingGet
 
This command will return the billing policy for the specified environment id, if one is associated, otherwise a 404 will be returned.

https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/environment-billing-policy/get-environment-billing-policy

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.
  - environmentId: (Optional) The environment id for which the operation will execute.  If not specified, all environments will be returned.

### CommandBillingPoliciesGet

This command will return the billing policies for the specified tenant.

https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/billing-policy/list-billing-policies

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.

### CommandBillingPolicyEnvironmentGet

This command will return the environments associated with the billing policy, if one is associated, otherwise a 404 will be returned.

https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/billing-policy-environment/list-billing-policy-environments

- Parameters:
  - tenantId: (Required) The tenant id for which the operation will execute.  This is used to retreive a user token.
  - billingPolicyId: (Required) The billing policy id for which the operation will execute.