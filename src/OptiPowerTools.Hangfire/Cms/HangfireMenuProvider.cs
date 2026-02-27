using EPiServer.Shell.Navigation;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Cms;

/// <summary>
/// Provides a menu item in the Optimizely CMS navigation for accessing the Hangfire dashboard.
/// The menu item links to the <see cref="HangfireCmsController"/> which renders the dashboard
/// embedded in the CMS shell.
/// </summary>
[MenuProvider]
public class HangfireMenuProvider : IMenuProvider
{
    private readonly OptiPowerToolHangfireOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="HangfireMenuProvider"/>.
    /// </summary>
    /// <param name="options">The Hangfire options for controlling menu visibility and roles.</param>
    public HangfireMenuProvider(IOptions<OptiPowerToolHangfireOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public IEnumerable<MenuItem> GetMenuItems()
    {
        if (!_options.EnableCmsMenu)
            return Enumerable.Empty<MenuItem>();

        var hangfireMenuItem = new UrlMenuItem(
            _options.DashboardTitle,
            MenuPaths.Global + "/cms/hangfire",
            "/HangfireCms/Index")
        {
            IsAvailable = _ => _options.AuthorizedRoles.Any(role =>
                EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole(role)),
            SortIndex = SortIndex.Last - 10
        };

        return [hangfireMenuItem];
    }
}
