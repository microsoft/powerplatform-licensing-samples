# CommandAllocationEnvironmentPatch

This command patches (modifies) the currency allocation for a specific environment. This is a **Legacy API** and is only supported for environments created before Tenant Pool licensing was introduced. You must be a **Tenant Admin** or **Environment Admin** to execute this command.

> **Warning:** Without `--whatif`, this command **will apply changes** to your environment.

API Reference: [Patch Currency Allocation by Environment](https://learn.microsoft.com/en-us/rest/api/power-platform/licensing/currency-allocation/patch-currency-allocation-by-environment)

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--environmentId` | | Yes | The Environment Id for which the allocation will be modified. |
| `--currencyType` | `-c` | Yes | The currency type to modify. Valid values: `AI`, `PowerAutomatePerProcess`, `ProcessMiningDataStorage`. |
| `--allocated` | `-a` | No | The numeric value for the allocation. Defaults to `0`. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints the changes that would be made without applying them. |

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

### 4. Preview the changes with WhatIf

Always start with `--whatif` to see what the command would do before applying changes:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentPatch" --tenantId <your-tenant-id> --environmentId <your-environment-id> -c PowerAutomatePerProcess -a 10 --whatif
```

**Example:**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentPatch" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02 -c PowerAutomatePerProcess -a 10 --whatif
```

The command will prompt you to authenticate with your tenant admin credentials, then print the changes that **would** be made without actually applying them.

### 5. Apply the changes

Once you have confirmed the preview, remove `--whatif` to apply:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentPatch" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 --environmentId d54a33bc-880a-e348-b341-3a0581585c02 -c PowerAutomatePerProcess -a 10
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID.
2. It retrieves the current allocations for the environment.
3. If the allocation already matches the requested value, the command reports no changes needed.
4. With `--whatif`: The command prints the PATCH request body showing the proposed changes.
5. Without `--whatif`: The command sends the PATCH request to update the allocation and prints the response.
