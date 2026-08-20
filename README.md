# OptiPowerTools.Hangfire

[![Quality Gate Status](https://sonarcloud.io/api/project_badges/measure?project=szolkowski_OptiPowerTools.Hangfire&metric=alert_status)](https://sonarcloud.io/summary/new_code?id=szolkowski_OptiPowerTools.Hangfire)
[![Coverage](https://sonarcloud.io/api/project_badges/measure?project=szolkowski_OptiPowerTools.Hangfire&metric=coverage)](https://sonarcloud.io/summary/new_code?id=szolkowski_OptiPowerTools.Hangfire)
[![Bugs](https://sonarcloud.io/api/project_badges/measure?project=szolkowski_OptiPowerTools.Hangfire&metric=bugs)](https://sonarcloud.io/summary/new_code?id=szolkowski_OptiPowerTools.Hangfire)
[![Code Smells](https://sonarcloud.io/api/project_badges/measure?project=szolkowski_OptiPowerTools.Hangfire&metric=code_smells)](https://sonarcloud.io/summary/new_code?id=szolkowski_OptiPowerTools.Hangfire)

A one-liner bootstrap for adding [Hangfire](https://www.hangfire.io/) background job processing to [Optimizely CMS 13](https://www.optimizely.com/). For Optimizely CMS 12 support, use the [1.x release](https://github.com/szolkowski/OptiPowerTools.Hangfire/tree/releases/v1-release).

This package was inspired by community feedback on the blog post [Adding Hangfire to Optimizely CMS 12](https://szolkowski.github.io/2024/07/31/adding-hangfire-to-epi-12.html), which walked through the manual steps of integrating Hangfire with Optimizely. The recurring request for a ready-made, drop-in solution led to this library — turning what was a multi-step manual setup into a simple, drop-in integration.

## Features

- Single extension method to register Hangfire with SQL Server storage and background server
- Hangfire Dashboard with Optimizely role-based authorization out of the box
- CMS menu integration — dashboard appears in the Optimizely navigation bar with configurable placement
- [Hangfire.Console](https://github.com/pieceofsummer/Hangfire.Console) support for rich job output
- Configurable via options pattern or appsettings.json
- Custom dashboard authorization — bring your own `IDashboardAuthorizationFilter` or disable auth entirely for development
- Toggle individual features on/off (`EnableDashboard`, `EnableConsole`, `EnableCmsMenu`)
- Built-in job filters for concurrency control (`MutualExclusion`, `WaitForOtherJobs`) and lifecycle management (`ExpireOnSuccess`, `RetainOnSuccess`)
- Targets net10.0 (Optimizely CMS 13 requires .NET 10)

![Hangfire Dashboard in Optimizely CMS](images/OptiToolsHangfireDashboard.png)

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

Connection string can point to the same database as Optimizely or to a separate one.

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
    options.DashboardPath = "/optimizely/backoffice/Plugins/hangfire";
    options.DashboardTitle = "OptiPowerTools Hangfire Dashboard";
    options.AuthorizedRoles = ["Administrators", "CmsAdmins", "WebAdmins"];
    options.SchemaName = "hangfire";
    options.EnableDashboard = true;
    options.EnableConsole = true;
    options.EnableCmsMenu = true;
    options.EnableStandardAuthorization = true;

    // Menu placement — default is CmsSection (under the CMS nav section)
    options.MenuPlacement = CmsMenuPlacement.CmsSection;
    options.MenuPath = null;        // Override the auto-derived menu path
    options.MenuSortIndex = null;   // Override the auto-derived sort index
    options.CustomSectionName = "OptiPowerTools"; // Section name for TopLevel/CustomSection placement
    options.CustomMenuItemName = "OptiPowerTools"; // Display name for the menu item
    options.CmsShellPath = "/HangfireCms/Index";   // URL path for the CMS shell iframe page

    // Storage maintenance
    options.JobExpirationCheckInterval = TimeSpan.FromMinutes(15);
});
```

### appsettings.json

```json
{
  "OptiPowerTools": {
    "Hangfire": {
      "ConnectionString": "Server=.;Database=MyDb;Trusted_Connection=True;",
      "DashboardPath": "/optimizely/backoffice/Plugins/hangfire",
      "DashboardTitle": "OptiPowerTools Hangfire Dashboard",
      "AuthorizedRoles": ["Administrators", "CmsAdmins", "WebAdmins"],
      "SchemaName": "hangfire",
      "EnableDashboard": true,
      "EnableConsole": true,
      "EnableCmsMenu": true,
      "EnableStandardAuthorization": true,
      "MenuPlacement": "CmsSection",
      "MenuPath": null,
      "MenuSortIndex": null,
      "CustomSectionName": "OptiPowerTools",
      "CustomMenuItemName": "OptiPowerTools",
      "CmsShellPath": "/HangfireCms/Index",
      "JobExpirationCheckInterval": "00:15:00"
    }
  }
}
```

### Options reference

| Option | Type | Default | Description |
| ------ | ---- | ------- | ----------- |
| `ConnectionString` | `string` | `""` | **Required.** SQL Server connection string for Hangfire storage. |
| `DashboardPath` | `string` | `"/optimizely/backoffice/Plugins/hangfire"` | URL path where the Hangfire dashboard is served. |
| `DashboardTitle` | `string` | `"OptiPowerTools Hangfire Dashboard"` | Title shown in the dashboard header. |
| `AuthorizedRoles` | `string[]` | `["Administrators", "CmsAdmins", "WebAdmins"]` | Optimizely roles allowed to access the dashboard. |
| `SchemaName` | `string` | `"hangfire"` | SQL Server schema for Hangfire tables. |
| `EnableDashboard` | `bool` | `true` | Serve the Hangfire dashboard UI. Set to `false` for worker-only nodes. |
| `EnableConsole` | `bool` | `true` | Enable Hangfire.Console for rich console output in jobs. |
| `EnableCmsMenu` | `bool` | `true` | Add a Hangfire menu item to the Optimizely CMS navigation. |
| `EnableStandardAuthorization` | `bool` | `true` | Use the built-in Optimizely role-based authorization filter for the dashboard. When `false` and no custom filter is provided, the dashboard allows unrestricted access. |
| `MenuPlacement` | `CmsMenuPlacement` | `CmsSection` | Where the menu item appears: `CmsSection`, `TopLevel`, or `CustomSection`. See [Menu Placement](#menu-placement). |
| `MenuPath` | `string?` | `null` | Overrides the auto-derived menu path. Takes precedence over `MenuPlacement` path logic. |
| `MenuSortIndex` | `int?` | `null` | Overrides the auto-derived sort index for the menu item (or section in `CustomSection` mode). |
| `CustomSectionName` | `string` | `"OptiPowerTools"` | Display name for the section group when `MenuPlacement` is `TopLevel` or `CustomSection`. |
| `CustomMenuItemName` | `string` | `"OptiPowerTools"` | Display name for the Hangfire menu item in the CMS navigation. Falls back to `DashboardTitle` when empty. |
| `CmsShellPath` | `string` | `"/HangfireCms/Index"` | URL path where the CMS shell page (iframe wrapper) is served. The CMS menu item links to this path. |
| `JobExpirationCheckInterval` | `TimeSpan` | `00:15:00` | How often the expiration manager checks for and removes expired jobs. |

### Dashboard Authorization

By default, the Hangfire dashboard is protected by the built-in Optimizely role-based authorization filter, which restricts access to users in the `AuthorizedRoles` (Administrators, CmsAdmins, WebAdmins). You can customize this behavior in three ways:

#### Standard authorization (default)

No changes needed. The dashboard uses `OptimizelyDashboardAuthorizationFilter` which checks the user's CMS roles.

#### Custom authorization filter

Provide your own `IDashboardAuthorizationFilter` implementation using the generic overload. The custom filter takes full precedence over the standard filter, regardless of the `EnableStandardAuthorization` setting.

```csharp
services.AddOptiPowerToolHangfire<MyCustomAuthFilter>(options =>
{
    options.ConnectionString = Configuration.GetConnectionString("HangfireConnection");
});
```

```csharp
public class MyCustomAuthFilter : IDashboardAuthorizationFilter
{
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return httpContext.User.Identity?.IsAuthenticated == true;
    }
}
```

#### Free access (no authorization)

Disable the standard authorization filter without providing a custom one. This allows unrestricted access to the dashboard — useful for development environments.

```csharp
services.AddOptiPowerToolHangfire(options =>
{
    options.ConnectionString = Configuration.GetConnectionString("HangfireConnection");
    options.EnableStandardAuthorization = false;
});
```

> **Warning:** Do not disable authorization in production. The Hangfire dashboard exposes job data, retry controls, and server information.

### Menu Placement

By default, the Hangfire menu item appears under the CMS section in the Optimizely navigation bar. You can change this with `MenuPlacement`:

#### `CmsSection` (default)

Nests the menu item under the existing CMS section. This is the current behavior and requires no configuration changes.

#### `TopLevel`

Places the menu item directly in the global navigation bar as a top-level entry, alongside CMS, Commerce, etc.

```json
{
  "OptiPowerTools": {
    "Hangfire": {
      "MenuPlacement": "TopLevel"
    }
  }
}
```

#### `CustomSection`

Creates a new collapsible section group and nests the Hangfire item underneath it. The section name is controlled by `CustomSectionName`.

```json
{
  "OptiPowerTools": {
    "Hangfire": {
      "MenuPlacement": "CustomSection",
      "CustomSectionName": "Background Jobs"
    }
  }
}
```

#### Overriding path and sort index

For any placement mode, you can override the menu path and sort index:

```json
{
  "OptiPowerTools": {
    "Hangfire": {
      "MenuPlacement": "CmsSection",
      "MenuPath": "/global/admin/hangfire",
      "MenuSortIndex": 500
    }
  }
}
```

## Job Filters

The package includes CMS-agnostic Hangfire job filters for concurrency control, available via the `OptiPowerTools.Hangfire.Tools.Filters` namespace.

### MutualExclusion

Prevents concurrent execution of jobs sharing the same resource group. Uses Hangfire distributed locks for reliable, race-condition-free mutual exclusion.

```csharp
using OptiPowerTools.Hangfire.Tools.Filters;

[MutualExclusion("data-pipeline")]
public class DataImportJob
{
    public void Execute() { /* ... */ }
}

[MutualExclusion("data-pipeline")]
public class DataExportJob
{
    public void Execute() { /* ... */ }
}
```

When `DataImportJob` is running, `DataExportJob` is automatically rescheduled (and vice versa). The worker thread is freed immediately — no blocking. The delay before retry is configurable:

```csharp
[MutualExclusion("data-pipeline", RetryDelaySeconds = 30)]
```

### WaitForOtherJobs

Prevents a job from executing while specific other job types are processing. This is one-directional — only the decorated job needs the attribute.

```csharp
using OptiPowerTools.Hangfire.Tools.Filters;

[WaitForOtherJobs(typeof(DataImportJob))]
public class ReportGeneratorJob
{
    public void Execute() { /* ... */ }
}
```

`ReportGeneratorJob` will be rescheduled if `DataImportJob` is currently processing. `DataImportJob` does not need any attribute and runs normally.

```csharp
// Wait for multiple job types, with custom retry delay
[WaitForOtherJobs(typeof(DataImportJob), typeof(DataExportJob), RetryDelaySeconds = 30)]
```

> **Note:** `WaitForOtherJobs` uses the Hangfire monitoring API and has a small race-condition window. For guaranteed mutual exclusion, use `MutualExclusion` instead.

### ExpireOnSuccess

Reduces the retention period for succeeded jobs. By default, Hangfire keeps succeeded jobs for 24 hours. This filter overrides the expiration timeout so short-lived fire-and-forget jobs are cleaned up faster.

```csharp
using OptiPowerTools.Hangfire.Tools.Filters;

// Job data expires 60 seconds after success (instead of 24 hours)
[ExpireOnSuccess(60)]
public class NotificationJob
{
    public void Execute() { /* ... */ }
}

// Job data expires immediately after success
[ExpireOnSuccess]
public class HealthCheckJob
{
    public void Execute() { /* ... */ }
}
```

The `expirationSeconds` parameter defaults to `0` (minimal retention). Only succeeded jobs are affected — failed or deleted jobs keep their default Hangfire expiration.

### RetainOnSuccess

Extends the retention period for succeeded jobs beyond Hangfire's default of 24 hours. Use this for infrequent jobs (weekly reports, monthly audits) where you want execution details to remain visible in the dashboard until the next run.

```csharp
using OptiPowerTools.Hangfire.Tools.Filters;

// Keep succeeded job data for 180 days
[RetainOnSuccess(180)]
public class MonthlyAuditJob
{
    public void Execute() { /* ... */ }
}

// Keep succeeded job data for 90 days (default)
[RetainOnSuccess]
public class WeeklyReportJob
{
    public void Execute() { /* ... */ }
}
```

The `retentionDays` parameter defaults to `90`. Only succeeded jobs are affected — failed or deleted jobs keep their default Hangfire expiration. For the inverse (reducing retention), see `ExpireOnSuccess`.

### Combining with DisableConcurrentExecution

Neither filter prevents the same job type from running concurrently with itself. Combine with Hangfire's built-in attribute if needed:

```csharp
[DisableConcurrentExecution(timeoutInSeconds: 60)]
[MutualExclusion("data-pipeline")]
public class DataImportJob { /* ... */ }
```

## Important: MapControllers requirement

The Hangfire CMS shell page (`/HangfireCms/Index`) is served by an MVC controller. If your application only calls `MapContent()` without `MapControllers()`, you must add it to your endpoint configuration:

```csharp
app.UseEndpoints(endpoints =>
{
    endpoints.MapContent();
    endpoints.MapControllers();
});

app.UseOptiPowerToolHangfire();
```

Most Optimizely CMS setups already include `MapControllers()`. If you see a **404 on `/HangfireCms/Index`**, this is the likely cause.

## Removing this package

This package is a thin configuration wrapper — it does not modify Hangfire internals or change the way Hangfire stores data. If your project outgrows it and you need full control, simply remove the package and configure Hangfire manually. Your existing database, jobs, and history will continue to work without any migration or data changes.

## Development

The solution includes a `.Web` project that references the [MyOptiAlloySite](https://github.com/szolkowski/MyOptiAlloySite) Optimizely CMS 13 site via a git submodule for manual testing. The site runs against SQL Server in Docker.

### Prerequisites

- .NET 10.0 SDK
- Docker (for SQL Server)
- Git with submodule support

### Getting started

1. Clone the repository with submodules:

   ```bash
   git clone --recursive https://github.com/szolkowski/OptiPowerTools.Hangfire.git
   ```

   If you already cloned without `--recursive`, initialize the submodule:

   ```bash
   git submodule update --init --recursive
   ```

2. Create the `.env` file (copy the example):

   ```bash
   cp .env.example .env
   ```

3. **Option A — Docker (recommended):** Start everything with docker compose from the repo root:

   ```bash
   docker compose up -d
   ```

   The site starts at `http://localhost:5100`.

   **Option B — Local:** Start only SQL Server in Docker, then run the site locally:

   ```bash
   docker compose up db -d
   dotnet run --project src/OptiPowerTools.Hangfire.Web
   ```

   The site starts at `https://localhost:5000`.

Once running:

| URL | Description |
| --- | --- |
| `/optimizely/cms` | CMS editorial UI |
| `/HangfireCms/Index` | Hangfire dashboard (embedded in CMS shell) |
| `/optimizely/backoffice/Plugins/hangfire` | Hangfire dashboard (standalone) |

### Docker compose reference

The `docker-compose.yml` at the repo root defines two services:

| Service | Description | Port |
| ------- | ----------- | ---- |
| `db` | SQL Server 2025 (reuses MyOptiAlloySite's Dockerfile) | `localhost:6000` |
| `web` | Hangfire Web project (.NET 10 SDK, live reload via volume mount) | `localhost:5100` |

**Common commands** (run from the repo root):

```bash
docker compose up -d              # Start both db and web
docker compose up -d --build      # Rebuild and start (after code changes)
docker compose up db -d           # Start only SQL Server (for local development)
docker compose restart web        # Restart web container
docker compose logs web --tail 50 # View recent web container logs
docker compose logs web -f        # Follow web container logs live
docker compose down               # Stop and remove containers
docker compose down -v            # Stop, remove containers, and delete volumes
```

**Environment configuration:** The `ASPNETCORE_ENVIRONMENT` variable in `docker-compose.yml` controls which `appsettings.{Environment}.json` is loaded. Change it to switch menu placement configurations:

| Value | Config file loaded | Menu placement |
| ----- | ------------------ | -------------- |
| `CmsSection` | `appsettings.CmsSection.json` | Under CMS section (default) |
| `TopLevel` | `appsettings.TopLevel.json` | Top-level nav entry |
| `CustomSection` | `appsettings.CustomSection.json` | Custom collapsible section |
| `Development` | `appsettings.Development.json` | Default ASP.NET Core dev environment |
| `NoSettingsConfig` | `appsettings.NoSettingsConfig.json` | No Hangfire options (defaults only) |

Connection strings are passed via Docker environment variables (`CONNECTIONSTRINGS__EPISERVERDB` and `OptiPowerTools__Hangfire__ConnectionString`), which override values in appsettings files.

**Rebuilding after code changes:** The web container mounts the repo as a volume and uses `dotnet run`, so it compiles on startup. After changing source files, rebuild the container:

```bash
docker compose up web -d --build
```

**After changing `docker-compose.yml`** (e.g. `ASPNETCORE_ENVIRONMENT`): A simple `restart` won't pick up compose file changes — you need to recreate the container:

```bash
docker compose up web -d --force-recreate
```

### Sample jobs

The `.Web` project includes sample jobs in the `Samples/` directory. These are not part of the NuGet package — they exist purely to showcase Hangfire and Hangfire.Console capabilities within Optimizely.

| Job | What it demonstrates |
| --- | -------------------- |
| `ConsoleShowcaseJob` | Hangfire.Console features: colored text, progress bars, and structured multi-phase output while processing fake product data |
| `OrderPipelineJob` | Job continuations via `IBackgroundJobClient.ContinueJobWith` — chains four steps (Validate → Payment → Ship → Notify) where each step is a separate job |
| `ScheduledCleanupJob` | Delayed execution via `IBackgroundJobClient.Schedule` — plans cleanup tasks with varying delays (1m, 5m, 15m) visible in the dashboard's Scheduled tab |
| `CancellableExportJob` | Cancellation token support via `IJobCancellationToken` — processes 500 records slowly enough to cancel mid-run from the dashboard and see graceful shutdown |

Trigger any sample job manually from the Hangfire dashboard's Recurring Jobs page, or wait for its schedule.

### Running tests

```bash
dotnet test
```

Tests run against `net10.0`.

### Project structure

| Project | Purpose |
| ------- | ------- |
| `src/OptiPowerTools.Hangfire` | The NuGet library package (`net10.0`) |
| `src/OptiPowerTools.Hangfire.Tools` | CMS-agnostic job filters and utilities (bundled into the main NuGet package) |
| `src/OptiPowerTools.Hangfire.Web` | Dev site for manual testing (`net10.0`, references MyOptiAlloySite submodule) |
| `tests/OptiPowerTools.Hangfire.Tests` | Unit tests for main library — xUnit + NSubstitute |
| `tests/OptiPowerTools.Hangfire.Tools.Tests` | Unit tests for Tools library — xUnit + NSubstitute |
| `sub/MyOptiAlloySite` | Git submodule — [szolkowski/MyOptiAlloySite](https://github.com/szolkowski/MyOptiAlloySite) (Optimizely CMS 13 Alloy site) |

## Version compatibility

| Package version | Optimizely CMS | .NET              |
| --------------- | -------------- | ----------------- |
| 2.x (current)   | CMS 13         | .NET 10           |
| 1.x             | CMS 12         | .NET 6, 8, 9, 10  |

**Optimizely CMS 12 users:** The 1.x line will continue to receive bug fixes and maintenance updates. You do not need to upgrade to 2.x unless you are migrating to CMS 13.

### Migrating from v1.x to v2.x

If you are upgrading from CMS 12 to CMS 13:

1. Update your project to target `net10.0`
2. Update Optimizely CMS packages to 13.x
3. Update the `OptiPowerTools.Hangfire` package to 2.x
4. If you customized `DashboardPath`, note the default changed from `/episerver/backoffice/Plugins/hangfire` to `/optimizely/backoffice/Plugins/hangfire`

## License

MIT. See [LICENSE](LICENSE.txt) for details.

## Contributing

Contributions are welcome! See [CONTRIBUTING.md](CONTRIBUTING.md) for guidelines..
