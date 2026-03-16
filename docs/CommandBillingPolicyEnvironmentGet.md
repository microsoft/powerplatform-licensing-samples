# CommandBillingPolicyEnvironmentGet

This command retrieves the environments associated with a specific billing policy. You must be a **Tenant Admin** to execute this command.

API Reference: [List Billing Policy Environments](https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/billing-policy-environment/list-billing-policy-environments)

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--billingPolicyId` | | Yes | The Billing Policy Id to look up associated environments for. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints the operation details without executing the API call. |

## Prerequisites

- [.NET 10.0 SDK](https://dotnet.microsoft.com/download) or later
- Tenant Admin permissions on the target tenant
- A valid Billing Policy Id (retrieve one using the [CommandBillingPoliciesGet](CommandBillingPoliciesGet.md) command)

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

Replace the placeholder values with your actual Tenant Id and Billing Policy Id.

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandBillingPolicyEnvironmentGet" --tenantId <your-tenant-id> --billingPolicyId <your-billing-policy-id>
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandBillingPolicyEnvironmentGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --billingPolicyId 1c590f8e-92ba-4f4b-84b4-2d048daebe34
```

The command will prompt you to authenticate with your tenant admin credentials via an interactive browser login. Once authenticated, it will print the environments linked to the specified billing policy.

### 5. Preview with WhatIf

To see the operation details without executing the API call:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandBillingPolicyEnvironmentGet" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --billingPolicyId 1c590f8e-92ba-4f4b-84b4-2d048daebe34 --whatif
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID.
2. It queries the Power Platform Licensing API for environments associated with the given billing policy.
3. The environment list is printed to the console. If no environments are associated, a 404 response is returned.
