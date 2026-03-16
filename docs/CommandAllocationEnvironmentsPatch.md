# CommandAllocationEnvironmentsPatch

This command enumerates **all environments** in your tenant and enables or disables the "Draw from Tenant Pool" licensing enforcement rule for each one. You must be a **Tenant Admin** to execute this command.

> **Warning:** Without `--whatif`, this command **will apply changes** to all environments in the tenant.

## Parameters

| Parameter | Short | Required | Description |
|---|---|---|---|
| `--tenantId` | `-t` | Yes | The Tenant Id used for authentication. This determines which tenant the operation targets. |
| `--action` | `-a` | No | The action to perform. Valid values: `DisableDrawFromTenantPool` (default), `EnableDrawFromTenantPool`. |
| `--whatif` | `-w` | No | Enables a "what would happen" preview. When present, the command prints all environments that would be modified without applying changes. |

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

### 4. Preview the changes with WhatIf

Always start with `--whatif` to see what environments would be affected:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentsPatch" --tenantId <your-tenant-id> -a DisableDrawFromTenantPool --whatif
```

**Example — Disable draw from Tenant Pool (preview):**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentsPatch" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 -a DisableDrawFromTenantPool --whatif
```

The command will prompt you to authenticate with your tenant admin credentials, then print all environments that would be modified along with the request body for each change.

**Example — Enable draw from Tenant Pool (preview):**

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentsPatch" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 -a EnableDrawFromTenantPool --whatif
```

### 5. Apply the changes

Once you have confirmed the preview output, remove `--whatif` to apply:

```bash
dotnet run --launch-profile "PlatformProd" --project src/sample.gateway "CommandAllocationEnvironmentsPatch" --tenantId 03ab3068-c403-406d-8351-bdbb6374c8b0 -a DisableDrawFromTenantPool
```

## What to Expect

1. The command authenticates you interactively via Microsoft Entra ID (you will be prompted for both gateway and BAP tokens).
2. It enumerates all environments in the tenant by paging through the BAP environment list.
3. For each environment, it retrieves the current allocation and enforcement rules.
4. It determines whether the `TenantPool` enforcement rule needs to be enabled or disabled based on the `--action` parameter.
5. Environments that already match the desired state are skipped.
6. With `--whatif`: For each environment requiring changes, the command prints the PUT request URL and body.
7. Without `--whatif`: The command sends PUT requests to update each environment and prints the responses.
