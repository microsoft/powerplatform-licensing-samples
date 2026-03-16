# CommandAllocationEnvironmentGet

This command retrieves the licensing currency allocation for a specific environment in your tenant. You must be a **Tenant Admin** or **Environment Admin** to execute this command.

API Reference: [Get Currency Allocation by Environment](https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/currency-allocation/get-currency-allocation-by-environment)

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--environmentId` | | Yes | The Environment Id for which the allocation will be retrieved. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints the operation details without executing the API call. |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Tenant Admin or Environment Admin permissions on the target tenant
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
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentGet" --tenantId <your-tenant-id> --environmentId <your-environment-id>
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02
```

The command will prompt you to authenticate with your tenant admin credentials via an interactive browser login. Once authenticated, it will print the currency allocations for the specified environment.

### 5. Preview with WhatIf

To preview the operation without making any changes:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02 --whatif
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID.
2. It queries the Power Platform Licensing API for currency allocations on the specified environment.
3. The allocation details (currency types and amounts) are printed to the console.
