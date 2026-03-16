# CommandCapacityGet

This command retrieves the tenant capacity currency reports for the specified tenant. You must be a **Tenant Admin** or **Environment Admin** to execute this command.

API Reference: [Get Tenant Capacity Details](https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/tenant-capacity-details/get-tenant-capacity-details)

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints the operation details without executing the API call. |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Tenant Admin or Environment Admin permissions on the target tenant

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

Replace the placeholder value with your actual Tenant Id.

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandCapacityGet" --tenantId <your-tenant-id>
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandCapacityGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0
```

The command will prompt you to authenticate with your tenant admin credentials via an interactive browser login. Once authenticated, it will print the capacity currency reports for your tenant.

### 5. Preview with WhatIf

To see the operation details without executing the API call:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandCapacityGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --whatif
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID.
2. It queries the Power Platform Licensing API for tenant capacity currency reports.
3. The capacity details (currency types, allocated amounts, consumed amounts) are printed to the console.
