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
        var defaultPathSuffix = string.IsNullOrEmpty(_options.MenuPath) ? "/cms/hangfire" : NormalizePath(_options.MenuPath);
        return BuildUrlMenuItem(defaultPathSuffix);
    }

    private List<MenuItem> BuildTopLevel()
    {
        var sectionName = string.IsNullOrEmpty(_options.CustomSectionName) ? _options.DashboardTitle : _options.CustomSectionName;
        var sectionSlug = ToSlug(sectionName);
        var sectionSortIndex = _options.MenuSortIndex ?? SortIndex.Last - 10;
        var sectionPath = string.IsNullOrEmpty(_options.MenuPath) ? "/" + sectionSlug : NormalizePath(_options.MenuPath);
        var itemPath = sectionPath + "/hangfire";

        var section = new SectionMenuItem(sectionName, MenuPaths.Global + sectionPath)
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = sectionSortIndex
        };

        var item = BuildUrlMenuItem(itemPath).First();

        return [section, item];
    }

    private List<MenuItem> BuildUrlMenuItem(string defaultPathSuffix)
    {
        var path = MenuPaths.Global + defaultPathSuffix;
        var sortIndex = _options.MenuSortIndex ?? SortIndex.Last - 10;
        var menuItemName = string.IsNullOrEmpty(_options.CustomMenuItemName) ? _options.DashboardTitle : _options.CustomMenuItemName;

        var item = new UrlMenuItem(menuItemName, path, _options.CmsShellPath)
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = sortIndex
        };

        return [item];
    }

    private List<MenuItem> BuildCustomSection()
    {
        var sectionName = string.IsNullOrEmpty(_options.CustomSectionName) ? _options.DashboardTitle : _options.CustomSectionName;
        var sectionSlug = ToSlug(sectionName);
        var sectionPath = MenuPaths.Global + (string.IsNullOrEmpty(_options.MenuPath) ? "/" + sectionSlug : NormalizePath(_options.MenuPath));
        var itemPath = sectionPath + "/hangfire";
        var menuItemName = string.IsNullOrEmpty(_options.CustomMenuItemName) ? _options.DashboardTitle : _options.CustomMenuItemName;
        var sectionSortIndex = _options.MenuSortIndex ?? SortIndex.Last - 10;

        var section = new SectionMenuItem(sectionName, sectionPath)
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = sectionSortIndex
        };

        var item = new UrlMenuItem(menuItemName, itemPath, _options.CmsShellPath)
        {
            IsAvailable = _ => IsCurrentUserAuthorized(),
            SortIndex = 100
        };

        return [section, item];
    }

    private bool IsCurrentUserAuthorized()
    {
        var principal = EPiServer.Security.PrincipalInfo.CurrentPrincipal;
        return principal?.Identity?.IsAuthenticated == true
            && _options.AuthorizedRoles is { } roles
            && roles.Any(principal.IsInRole);
    }

    private static string NormalizePath(string path) =>
        path.StartsWith('/') ? path : "/" + path;

    private static string ToSlug(string name)
    {
        var slug = name.ToLowerInvariant()
            .Replace(' ', '-')
            .Replace('_', '-')
            .Replace('.', '-');
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"[^a-z0-9\-]", "");
        slug = System.Text.RegularExpressions.Regex.Replace(slug, @"-{2,}", "-");
        return slug.Trim('-');
    }
}
