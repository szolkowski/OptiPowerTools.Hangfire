using Hangfire;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Authorization;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Extensions;

/// <summary>
/// Extension methods for configuring the OptiPowerTools Hangfire middleware pipeline.
/// </summary>
public static class ApplicationBuilderExtensions
{
    /// <summary>
    /// Adds the Hangfire dashboard middleware to the application pipeline.
    /// Must be called after UseAuthentication() and UseAuthorization().
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseOptiPowerToolHangfire(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices
            .GetRequiredService<IOptions<OptiPowerToolHangfireOptions>>().Value;

        if (options.EnableDashboard)
        {
            var authFilter = app.ApplicationServices
                .GetRequiredService<OptimizelyDashboardAuthorizationFilter>();

            var dashboardOptions = new DashboardOptions
            {
                DashboardTitle = options.DashboardTitle,
                Authorization = [authFilter],
                AppPath = null
            };

            app.UseHangfireDashboard(options.DashboardPath, dashboardOptions);
        }

        return app;
    }
}
