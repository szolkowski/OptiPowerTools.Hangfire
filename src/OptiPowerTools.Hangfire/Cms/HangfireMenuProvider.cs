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

        return _options.MenuPlacement switch
        {
            CmsMenuPlacement.TopLevel => BuildTopLevel(),
            CmsMenuPlacement.CustomSection => BuildCustomSection(),
            _ => BuildCmsSection()
        };
    }

    private List<MenuItem> BuildCmsSection()
    {
        var path = _options.MenuPath ?? MenuPaths.Global + "/cms/hangfire";
        var sortIndex = _options.MenuSortIndex ?? SortIndex.Last - 10;

        var item = new UrlMenuItem(_options.DashboardTitle, path, "/HangfireCms/Index")
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = sortIndex
        };

        return [item];
    }

    private List<MenuItem> BuildTopLevel()
    {
        var path = _options.MenuPath ?? MenuPaths.Global + "/hangfire";
        var sortIndex = _options.MenuSortIndex ?? SortIndex.Last - 10;

        var item = new UrlMenuItem(_options.DashboardTitle, path, "/HangfireCms/Index")
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = sortIndex
        };

        return [item];
    }

    private List<MenuItem> BuildCustomSection()
    {
        var sectionName = _options.CustomSectionName;
        var sectionSlug = ToSlug(sectionName);
        var sectionPath = _options.MenuPath ?? MenuPaths.Global + "/" + sectionSlug;
        var itemPath = sectionPath + "/hangfire";
        var sectionSortIndex = _options.MenuSortIndex ?? SortIndex.Last - 10;

        var section = new SectionMenuItem(sectionName, sectionPath)
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = sectionSortIndex
        };

        var item = new UrlMenuItem(_options.DashboardTitle, itemPath, "/HangfireCms/Index")
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = 100
        };

        return [section, item];
    }

    private bool IsCurrentUserAuthorized()
    {
        return _options.AuthorizedRoles.Any(role =>
            EPiServer.Security.PrincipalInfo.CurrentPrincipal.IsInRole(role));
    }

    private static string ToSlug(string name)
    {
        return name.ToLowerInvariant()
            .Replace(" ", "-")
            .Replace("_", "-");
    }
}
