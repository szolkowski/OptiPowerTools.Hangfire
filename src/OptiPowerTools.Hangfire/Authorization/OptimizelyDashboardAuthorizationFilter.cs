using System.Security.Claims;
using Hangfire.Dashboard;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Authorization;

/// <summary>
/// Hangfire dashboard authorization filter that restricts access based on Optimizely CMS roles.
/// By default, only users in Administrators, CmsAdmins, or WebAdmins roles can access the dashboard.
/// </summary>
public class OptimizelyDashboardAuthorizationFilter : IDashboardAuthorizationFilter
{
    private readonly OptiPowerToolHangfireOptions _options;

    /// <summary>
    /// Initializes a new instance of <see cref="OptimizelyDashboardAuthorizationFilter"/>.
    /// </summary>
    /// <param name="options">The Hangfire options containing authorized roles.</param>
    public OptimizelyDashboardAuthorizationFilter(IOptions<OptiPowerToolHangfireOptions> options)
    {
        _options = options.Value;
    }

    /// <inheritdoc />
    public bool Authorize(DashboardContext context)
    {
        var httpContext = context.GetHttpContext();
        return IsAuthorized(httpContext.User);
    }

    /// <summary>
    /// Checks whether the given principal is authorized to access the Hangfire dashboard.
    /// </summary>
    /// <param name="user">The claims principal to check.</param>
    /// <returns>True if the user is authenticated and in at least one authorized role.</returns>
    internal bool IsAuthorized(ClaimsPrincipal user)
    {
        return user.Identity?.IsAuthenticated == true
            && _options.AuthorizedRoles.Any(role => user.IsInRole(role));
    }
}
