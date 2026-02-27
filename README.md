# OptiPowerTools.Hangfire

A one-liner bootstrap for adding [Hangfire](https://www.hangfire.io/) background job processing to [Optimizely CMS 12](https://www.optimizely.com/).

## Features

- Single extension method to register Hangfire with SQL Server storage and background server
- Hangfire Dashboard with Optimizely role-based authorization out of the box
- CMS menu integration — dashboard appears in the Optimizely navigation bar
- [Hangfire.Console](https://github.com/pieceofsummer/Hangfire.Console) support for rich job output
- Configurable via options pattern or appsettings.json
- Toggle individual features on/off (`EnableDashboard`, `EnableConsole`, `EnableCmsMenu`)
- Targets net8.0

## Quick Start

```csharp
// In Program.cs or Startup.cs
services.AddOptiPowerToolHangfire(options =>
{
    options.ConnectionString = Configuration.GetConnectionString("HangfireConnection");
});

// In the middleware pipeline (after UseAuthentication/UseAuthorization)
app.UseOptiPowerToolHangfire();
```

Connection string can point to the same database as Optimizely or to separate one.

That's it. This registers Hangfire with SQL Server storage, starts the background server, enables the dashboard with role-based auth, and adds a menu item to the CMS navigation.

## Configuration

All options except `ConnectionString` have sensible defaults. Configure via code, appsettings.json, or both (code overrides config).

### Code configuration

#### Minimal configuration

```csharp
// Connection string is read from appsettings.json ("OptiPowerTools:Hangfire:ConnectionString")
services.AddOptiPowerToolHangfire();

app.UseOptiPowerToolHangfire();
```

#### Full configuration

```csharp
services.AddOptiPowerToolHangfire(options =>
{
    // Required
    options.ConnectionString = "Server=.;Database=MyDb;Trusted_Connection=True;";

    // Optional — all values below are the defaults
    options.DashboardPath = "/episerver/backoffice/Plugins/hangfire";
    options.DashboardTitle = "OptiPowerTools Hangfire Dashboard";
    options.AuthorizedRoles = ["Administrators", "CmsAdmins", "WebAdmins"];
    options.SchemaName = "hangfire";
    options.EnableDashboard = true;
    options.EnableConsole = true;
    options.EnableCmsMenu = true;
});
```

### appsettings.json

```json
{
  "OptiPowerTools": {
    "Hangfire": {
      "ConnectionString": "Server=.;Database=MyDb;Trusted_Connection=True;",
      "DashboardPath": "/episerver/backoffice/Plugins/hangfire",
      "DashboardTitle": "OptiPowerTools Hangfire Dashboard",
      "AuthorizedRoles": ["Administrators", "CmsAdmins", "WebAdmins"],
      "SchemaName": "hangfire",
      "EnableDashboard": true,
      "EnableConsole": true,
      "EnableCmsMenu": true
    }
  }
}
```

### Options reference

| Option | Type | Default | Description |
|--------|------|---------|-------------|
| `ConnectionString` | `string` | `""` | **Required.** SQL Server connection string for Hangfire storage. |
| `DashboardPath` | `string` | `"/episerver/backoffice/Plugins/hangfire"` | URL path where the Hangfire dashboard is served. |
| `DashboardTitle` | `string` | `"OptiPowerTools Hangfire Dashboard"` | Title shown in the dashboard header. |
| `AuthorizedRoles` | `string[]` | `["Administrators", "CmsAdmins", "WebAdmins"]` | Optimizely roles allowed to access the dashboard. |
| `SchemaName` | `string` | `"hangfire"` | SQL Server schema for Hangfire tables. |
| `EnableDashboard` | `bool` | `true` | Serve the Hangfire dashboard UI. Set to `false` for worker-only nodes. |
| `EnableConsole` | `bool` | `true` | Enable Hangfire.Console for rich console output in jobs. |
| `EnableCmsMenu` | `bool` | `true` | Add a Hangfire menu item to the Optimizely CMS navigation. |

## Removing this package

This package is a thin configuration wrapper — it does not modify Hangfire internals or change the way Hangfire stores data. If your project outgrows it and you need full control, simply remove the package and configure Hangfire manually. Your existing database, jobs, and history will continue to work without any migration or data changes.

## Development

The solution includes a `.Web` project that references the [Optimizely Foundation](https://github.com/episerver/Foundation) site via a git submodule for manual testing.

### Prerequisites

- .NET 8.0 SDK
- Docker (for SQL Server)
- Git with submodule support

### Getting started

1. Clone the repository with submodules:

   ```bash
   git clone --recursive https://github.com/<owner>/OptiPowerTools.Hangfire.git
   ```

   If you already cloned without `--recursive`, initialize the submodule:

   ```bash
   git submodule update --init --recursive
   ```

   If you don't have Foundation DB configured follow it readme and add connection strings to `/Users/sszolkowski/repos/OptiPowerTools.Hangfire/src/OptiPowerTools.Hangfire.Web/appsettings.json` or `src/OptiPowerTools.Hangfire.Web/appsettings.Development.json`.

2. Build and run:

   ```bash
   dotnet build
   dotnet run --project src/OptiPowerTools.Hangfire.Web
   ```

The site starts at `https://localhost:5001` or `http://localhost:5000`. Once running:

| URL | Description |
| --- | --- |
| `/` | Foundation home page |
| `/util/login` | CMS admin login |
| `/episerver/cms` | CMS editorial UI |
| `/HangfireCms/Index` | Hangfire dashboard (embedded in CMS shell) |
| `/episerver/backoffice/Plugins/hangfire` | Hangfire dashboard (standalone) |

### Running tests

```bash
dotnet test
```

Tests run against `net8.0`.

### Project structure

| Project | Purpose |
|---------|---------|
| `src/OptiPowerTools.Hangfire` | The NuGet library package (`net8.0`) |
| `src/OptiPowerTools.Hangfire.Web` | Dev site for manual testing (`net8.0`, references Foundation submodule) |
| `tests/OptiPowerTools.Hangfire.Tests` | Unit tests — xUnit + NSubstitute (`net8.0`) |
| `sub/foundation` | Git submodule — [episerver/Foundation](https://github.com/episerver/Foundation) |

### Troubleshooting

- **`BinaryFormatter serialization ... have been removed`** — The project must target `net8.0`. Foundation's Commerce modules require `BinaryFormatter`.

## License

Apache-2.0. See [LICENSE](LICENSE) for details.
