# CommandBillingPoliciesGet

This command retrieves all billing policies for the specified tenant. You must be a **Tenant Admin** to execute this command.

API Reference: [List Billing Policies](https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/billing-policy/list-billing-policies)

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints the operation details without executing the API call. |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Tenant Admin permissions on the target tenant

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
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandBillingPoliciesGet" --tenantId <your-tenant-id>
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandBillingPoliciesGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0
```

The command will prompt you to authenticate with your tenant admin credentials via an interactive browser login. Once authenticated, it will print all billing policies for your tenant.

### 5. Preview with WhatIf

To see the operation details without executing the API call:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandBillingPoliciesGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --whatif
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID.
2. It queries the Power Platform Licensing API for billing policies associated with the tenant.
3. The billing policy details are printed to the console.
