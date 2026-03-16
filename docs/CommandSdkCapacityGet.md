# CommandSdkCapacityGet

This command retrieves the tenant capacity currency reports using the **Power Platform Management SDK** instead of direct HTTP calls. This demonstrates an alternative approach to calling the Licensing APIs via the official SDK client. You must be a **Tenant Admin** or **Environment Admin** to execute this command.

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
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandSdkCapacityGet" --tenantId <your-tenant-id>
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandSdkCapacityGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0
```

The command will prompt you to authenticate with your tenant admin credentials. Once authenticated, it will retrieve the capacity currency reports using the Power Platform Management SDK.

### 5. Preview with WhatIf

To see the operation details without executing the API call:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandSdkCapacityGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --whatif
```

## What to Expect

1. The command initializes the Power Platform Management SDK client.
2. It authenticates you using the Microsoft PowerShell public client application.
3. It calls the SDK's `Licensing.TenantCapacity.CurrencyReports.GetAsync()` method to retrieve capacity data.
4. The capacity currency reports are printed to the console.

## How This Differs from CommandCapacityGet

| | CommandCapacityGet | CommandSdkCapacityGet |
|---|---|---|
| Approach | Direct HTTP calls to the gateway API | Uses the Power Platform Management SDK |
| Authentication | MSAL interactive token with gateway routing | SDK-managed authentication |
| Use case | Low-level API demonstration | SDK integration demonstration |
