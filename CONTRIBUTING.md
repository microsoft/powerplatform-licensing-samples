# Contributing

Thank you for your interest in contributing to Power Platform Licensing Samples.

## Reporting issues

Use GitHub Issues to report bugs or suggest improvements. Include the command you ran, the expected and actual behavior, and any relevant error output. Do not include credentials, access tokens, or other sensitive information.

## Submitting changes

1. Open an issue to discuss substantial changes before starting work.
2. Create a focused branch and make changes that address one issue.
3. Add or update tests and documentation as appropriate.
4. Run the tests and build before opening a pull request:

   ```bash
   dotnet test src/sample.gateway.sln
   dotnet build src/sample.gateway.sln -c Debug -a x64
   ```

5. Open a pull request describing the change, linking the related issue, and summarizing how it was tested.

Please follow the [Microsoft Open Source Code of Conduct](./CODE_OF_CONDUCT.md). For security issues, follow the instructions in [SECURITY.md](./SECURITY.md) instead of opening a public issue.
