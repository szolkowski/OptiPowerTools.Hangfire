# Contributing to OptiPowerTools.Hangfire

Thank you for your interest in contributing! This guide will help you get started.

## Getting Started

1. Fork the repository and clone:

   ```bash
   git clone https://github.com/<your-username>/OptiPowerTools.Hangfire.git
   ```

2. Create a branch for your change:

   ```bash
   git checkout -b feature/your-feature-name
   ```

3. Build and run tests:

   ```bash
   dotnet build
   dotnet test
   ```

## Development Setup

- **.NET SDK required:** 10.0
- **Docker** is needed for SQL Server (used by the dev web site)

See the [README](README.md#development) for full setup instructions.

## Code Style

- **C#:** File-scoped namespaces, Allman braces, 4-space indent, `var` for apparent types
- **JSON/XML/YAML:** 2-space indent
- **Naming:** `_camelCase` for private fields, `PascalCase` for public members
- **Patterns:** Options pattern (`IOptions<T>`), DI via extension methods, expression bodies preferred, pattern matching over casts
- **Warnings as errors** is enabled — your code must compile warning-free

## Testing

- Use xUnit with NSubstitute for mocking
- Follow the AAA pattern (Arrange, Act, Assert)
- Use `[Theory]`/`[InlineData]` for parameterized tests
- Tests run against net10.0

Run the full test suite before submitting:

```bash
dotnet test
```

## Submitting Changes

1. Ensure all tests pass and the build is warning-free
2. Keep commits focused — one logical change per commit
3. Write clear commit messages that explain *why*, not just *what*
4. Open a pull request against the `main` branch
5. Describe what your PR does and why in the PR description

## Reporting Issues

- Use [GitHub Issues](https://github.com/szolkowski/OptiPowerTools.Hangfire/issues) to report bugs or suggest features
- Include steps to reproduce for bug reports
- Mention which .NET version and Optimizely CMS version you are using

## License

By contributing, you agree that your contributions will be licensed under the [MIT License](LICENSE.txt).