using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Routing;
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

        // Ensure the HangfireCmsController route is mapped even if the host
        // only calls MapContent() without MapControllers().
        // The try/catch handles cases where UseRouting() or AddControllers()
        // haven't been called yet (e.g. unit tests or minimal setups).
        try
        {
            app.UseEndpoints(endpoints => endpoints.MapControllers());
        }
        catch (InvalidOperationException)
        {
            // Routing or controller services not registered — the host app
            // is responsible for mapping controllers in this case.
        }

        if (options.EnableDashboard)
        {
            var dashboardOptions = new DashboardOptions
            {
                DashboardTitle = options.DashboardTitle,
                Authorization = ResolveAuthorizationFilters(app.ApplicationServices, options),
                AppPath = null
            };

            app.UseHangfireDashboard(options.DashboardPath, dashboardOptions);
        }

        return app;
    }

    internal static IEnumerable<IDashboardAuthorizationFilter> ResolveAuthorizationFilters(
        IServiceProvider serviceProvider,
        OptiPowerToolHangfireOptions options)
    {
        var customFilter = serviceProvider.GetService<IDashboardAuthorizationFilter>();

        if (customFilter is not null)
            return [customFilter];

        if (options.EnableStandardAuthorization)
        {
            var standardFilter = serviceProvider
                .GetRequiredService<OptimizelyDashboardAuthorizationFilter>();
            return [standardFilter];
        }

        return [];
    }
}
