namespace OptiPowerTools.Hangfire.Configuration;

/// <summary>
/// Controls where the Hangfire menu item appears in Optimizely CMS navigation.
/// </summary>
public enum CmsMenuPlacement
{
    /// <summary>
    /// Places the Hangfire menu item as a sub-entry under the existing CMS section.
    /// Default path: <c>MenuPaths.Global + "/cms/hangfire"</c>.
    /// This is the default behavior.
    /// </summary>
    CmsSection = 0,

    /// <summary>
    /// Places the Hangfire menu item directly in the global navigation bar as a top-level entry.
    /// Default path: <c>MenuPaths.Global + "/hangfire"</c>.
    /// </summary>
    TopLevel = 1,

    /// <summary>
    /// Creates a new section group and nests the Hangfire menu item underneath it.
    /// The section name is controlled by <see cref="OptiPowerToolHangfireOptions.CustomSectionName"/>.
    /// </summary>
    CustomSection = 2
}
