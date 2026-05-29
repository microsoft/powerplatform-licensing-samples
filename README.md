# Neptune Licensing Sampleware

Sample code for running API calls against Neptune via the Power Platform API Gateway.

## Getting Started

### 1. Install .NET 10 SDK

```powershell
winget install Microsoft.DotNet.SDK.10
```

Or download directly from [dot.net](https://dotnet.microsoft.com/download/dotnet/10.0).

### 2. Restore packages

```bash
dotnet restore src/sample.gateway/sample.gateway.csproj
```

### 3. Build

```bash
dotnet build src/sample.gateway/sample.gateway.csproj -c Debug -a x64
```

### 4. List available commands

```bash
dotnet run --project src/sample.gateway -- --help
```

This prints all available commands with their options. To get help for a specific command:

```bash
dotnet run --project src/sample.gateway -- <CommandName> --help
```

## Command Documentation

| Command | Description |
|---|---|
| [CommandAllocationEnvironmentGet](docs/CommandAllocationEnvironmentGet.md) | Get currency allocation for an environment |
| [CommandAllocationEnvironmentPatch](docs/CommandAllocationEnvironmentPatch.md) | Patch currency allocation for an environment |
| [CommandAllocationEnvironmentsPatch](docs/CommandAllocationEnvironmentsPatch.md) | Patch Draw from Tenant Pool enforcement across all environments |
| [CommandBillingPoliciesGet](docs/CommandBillingPoliciesGet.md) | List billing policies for a tenant |
| [CommandBillingPolicyEnvironmentGet](docs/CommandBillingPolicyEnvironmentGet.md) | List environments for a billing policy |
| [CommandCapacityGet](docs/CommandCapacityGet.md) | Get tenant capacity currency reports |
| [CommandEnvironmentBillingGet](docs/CommandEnvironmentBillingGet.md) | Get billing policy for an environment |
| [CommandSdkCapacityGet](docs/CommandSdkCapacityGet.md) | Get tenant capacity reports via the Power Platform Management SDK |

## Contributing

We welcome contributions. Please file issues for bugs, enhancements, or documentation improvements.

## Code of Conduct

This project follows the [Microsoft Open Source Code of Conduct](https://opensource.microsoft.com/codeofconduct/).

## License

Licensed under the [MIT License](./LICENSE.md).

---

_Trademarks: This project may include trademarks or logos for Microsoft or third parties. Use of Microsoft trademarks or logos must follow [Microsoft's Trademark & Brand Guidelines](https://www.microsoft.com/en-us/legal/intellectualproperty/trademarks/usage/general). Third-party trademarks are subject to their respective policies._
