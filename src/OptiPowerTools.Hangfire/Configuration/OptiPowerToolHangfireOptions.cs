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
    /// Whether to add a menu item in the Optimizely CMS navigation for the Hangfire dashboard.
    /// Defaults to true.
    /// </summary>
    public bool EnableCmsMenu { get; set; } = true;
}
