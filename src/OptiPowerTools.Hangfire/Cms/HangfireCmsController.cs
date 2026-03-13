using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Cms;

/// <summary>
/// MVC controller that renders the Hangfire dashboard within the Optimizely CMS shell.
/// The view embeds the Hangfire dashboard in an iframe with the CMS navigation chrome.
/// </summary>
[Authorize]
[Route("[controller]")]
public class HangfireCmsController : Controller
{
    private readonly OptiPowerToolHangfireOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="HangfireCmsController"/>.
    /// </summary>
    /// <param name="options">The Hangfire options for dashboard path and title.</param>
    public HangfireCmsController(IOptions<OptiPowerToolHangfireOptions> options)
    {
        _options = options.Value;
    }

    /// <summary>
    /// Renders the Hangfire dashboard embedded in the CMS shell.
    /// </summary>
    [Route("[action]")]
    public IActionResult Index()
    {
        if (User.Identity?.IsAuthenticated != true
            || _options.AuthorizedRoles is not { } roles
            || !roles.Any(role => User.IsInRole(role)))
            return Forbid();

        ViewBag.DashboardPath = _options.DashboardPath;
        ViewBag.DashboardTitle = _options.DashboardTitle;
        return View();
    }
}
