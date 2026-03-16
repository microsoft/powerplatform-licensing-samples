# CommandEnvironmentBillingGet

This command retrieves the billing policy associated with a specific environment. If no billing policy is linked to the environment, a 404 response is returned. You must be a **Tenant Admin** to execute this command.

API Reference: [Get Environment Billing Policy](https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/environment-billing-policy/get-environment-billing-policy)

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--environmentId` | | Yes | The Environment Id for which the billing policy will be retrieved. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints the operation details without executing the API call. |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Tenant Admin permissions on the target tenant
- A valid Environment Id (found in the [Power Platform Admin Center](https://admin.powerplatform.microsoft.com))

## Step-by-Step

### 1. Clone the repository

```bash
git clone <repository-url>
cd powerplatform-licensing-samples
```

### 2. Restore packages

```bash
dotnet restore src/sample.gateway/sample.gateway.csproj
```

### 3. Build the project

```bash
dotnet build src/sample.gateway/sample.gateway.csproj
```

### 4. Run the command

Replace the placeholder values with your actual Tenant Id and Environment Id.

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandEnvironmentBillingGet" --tenantId <your-tenant-id> --environmentId <your-environment-id>
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandEnvironmentBillingGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02
```

The command will prompt you to authenticate with your tenant admin credentials via an interactive browser login. Once authenticated, it will print the billing policy details for the specified environment.

### 5. Preview with WhatIf

To see the operation details without executing the API call:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandEnvironmentBillingGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02 --whatif
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID.
2. It queries the Power Platform Licensing API for the billing policy linked to the specified environment.
3. The billing policy details are printed to the console. If no billing policy is associated with the environment, a 404 is returned.
