namespace OptiPowerTools.Hangfire.Configuration;

/// <summary>
/// Configuration options for the OptiPowerTools Hangfire integration.
/// </summary>
public class OptiPowerToolHangfireOptions
{
    /// <summary>
    /// The SQL Server connection string used by Hangfire for job storage.
    /// This is required and must be set before the application starts.
    /// </summary>
    public string ConnectionString { get; set; } = string.Empty;

    /// <summary>
    /// The URL path where the Hangfire dashboard will be accessible.
    /// Defaults to "/episerver/backoffice/Plugins/hangfire".
    /// </summary>
    public string DashboardPath { get; set; } = "/episerver/backoffice/Plugins/hangfire";

    /// <summary>
    /// The title displayed in the Hangfire dashboard header.
    /// Defaults to "OptiPowerTools Hangfire Dashboard".
    /// </summary>
    public string DashboardTitle { get; set; } = "OptiPowerTools Hangfire Dashboard";

    /// <summary>
    /// The Optimizely/EPiServer roles that are authorized to access the Hangfire dashboard.
    /// Defaults to Administrators, CmsAdmins, and WebAdmins.
    /// </summary>
    public string[] AuthorizedRoles { get; set; } = ["Administrators", "CmsAdmins", "WebAdmins"];

    /// <summary>
    /// The schema name used for Hangfire SQL Server storage tables.
    /// Defaults to "hangfire".
    /// </summary>
    public string SchemaName { get; set; } = "hangfire";

    /// <summary>
    /// Whether to enable the Hangfire dashboard middleware.
    /// When false, the dashboard UI is not served but background processing continues.
    /// Defaults to true.
    /// </summary>
    public bool EnableDashboard { get; set; } = true;

    /// <summary>
    /// Whether to enable the Hangfire.Console extension for rich console output in jobs.
    /// Defaults to true.
    /// </summary>
    public bool EnableConsole { get; set; } = true;

    /// <summary>
    /// Whether to use the built-in Optimizely role-based authorization filter for the dashboard.
    /// When false and no custom filter is provided via <c>AddOptiPowerToolHangfire&lt;TFilter&gt;</c>,
    /// the dashboard allows unrestricted access.
    /// Defaults to true.
    /// </summary>
    public bool EnableStandardAuthorization { get; set; } = true;

    /// <summary>
    /// Whether to add a menu item in the Optimizely CMS navigation for the Hangfire dashboard.
    /// Defaults to true.
    /// </summary>
    public bool EnableCmsMenu { get; set; } = true;

    /// <summary>
    /// Controls where the Hangfire menu item is placed in the CMS navigation.
    /// Defaults to <see cref="CmsMenuPlacement.CmsSection"/>, which nests it under the CMS section.
    /// </summary>
    public CmsMenuPlacement MenuPlacement { get; set; } = CmsMenuPlacement.CmsSection;

    /// <summary>
    /// Overrides the full menu path for the Hangfire menu item.
    /// When null (the default), the path is derived automatically from <see cref="MenuPlacement"/>.
    /// When set, this value takes precedence over the derived path regardless of placement mode.
    /// </summary>
    public string? MenuPath { get; set; }

    /// <summary>
    /// Overrides the sort index for the Hangfire menu item.
    /// When null (the default), a sensible default is chosen based on <see cref="MenuPlacement"/>.
    /// </summary>
    public int? MenuSortIndex { get; set; }

    /// <summary>
    /// The display name for the section when <see cref="MenuPlacement"/> is
    /// <see cref="CmsMenuPlacement.TopLevel"/> or <see cref="CmsMenuPlacement.CustomSection"/>.
    /// Defaults to "OptiPowerTools". Ignored when <see cref="MenuPlacement"/> is
    /// <see cref="CmsMenuPlacement.CmsSection"/> (no section is created).
    /// </summary>
    public string CustomSectionName { get; set; } = "OptiPowerTools";
    
    /// <summary>
    /// The display name for the Hangfire menu item in the CMS navigation.
    /// When empty or null, falls back to <see cref="DashboardTitle"/>.
    /// </summary>
    public string CustomMenuItemName { get; set; } = string.Empty;

    /// <summary>
    /// The interval at which the expiration manager checks for and removes expired jobs.
    /// Defaults to 15 minutes.
    /// </summary>
    public TimeSpan JobExpirationCheckInterval { get; set; } = TimeSpan.FromMinutes(15);
}
