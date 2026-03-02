using Hangfire;
using Hangfire.Console;
using Hangfire.Dashboard;
using Hangfire.SqlServer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Authorization;
using OptiPowerTools.Hangfire.Cms;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Extensions;

/// <summary>
/// Extension methods for registering OptiPowerTools Hangfire services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Hangfire services configured for Optimizely CMS with default options.
    /// Uses the built-in Optimizely role-based authorization filter for the dashboard.
    /// Connection string must be provided via appsettings.json under "OptiPowerTools:Hangfire:ConnectionString".
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiPowerToolHangfire(this IServiceCollection services) =>
        services.AddOptiPowerToolHangfire(_ => { });

    /// <summary>
    /// Adds Hangfire services configured for Optimizely CMS with the specified options.
    /// Uses the built-in Optimizely role-based authorization filter for the dashboard.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="setupAction">An action to configure <see cref="OptiPowerToolHangfireOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiPowerToolHangfire(
        this IServiceCollection services,
        Action<OptiPowerToolHangfireOptions> setupAction)
    {
        AddCoreServices(services, setupAction);
        return services;
    }

    /// <summary>
    /// Adds Hangfire services configured for Optimizely CMS with a custom dashboard authorization filter.
    /// The custom filter takes precedence over the built-in Optimizely role-based filter,
    /// regardless of the <see cref="OptiPowerToolHangfireOptions.EnableStandardAuthorization"/> setting.
    /// </summary>
    /// <typeparam name="TFilter">
    /// A custom <see cref="IDashboardAuthorizationFilter"/> implementation to use for dashboard access control.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiPowerToolHangfire<TFilter>(this IServiceCollection services)
        where TFilter : class, IDashboardAuthorizationFilter =>
        services.AddOptiPowerToolHangfire<TFilter>(_ => { });

    /// <summary>
    /// Adds Hangfire services configured for Optimizely CMS with a custom dashboard authorization filter
    /// and the specified options. The custom filter takes precedence over the built-in Optimizely role-based filter,
    /// regardless of the <see cref="OptiPowerToolHangfireOptions.EnableStandardAuthorization"/> setting.
    /// </summary>
    /// <typeparam name="TFilter">
    /// A custom <see cref="IDashboardAuthorizationFilter"/> implementation to use for dashboard access control.
    /// </typeparam>
    /// <param name="services">The service collection.</param>
    /// <param name="setupAction">An action to configure <see cref="OptiPowerToolHangfireOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiPowerToolHangfire<TFilter>(
        this IServiceCollection services,
        Action<OptiPowerToolHangfireOptions> setupAction)
        where TFilter : class, IDashboardAuthorizationFilter
    {
        AddCoreServices(services, setupAction);
        services.AddSingleton<IDashboardAuthorizationFilter, TFilter>();
        return services;
    }

    private static void AddCoreServices(
        IServiceCollection services,
        Action<OptiPowerToolHangfireOptions> setupAction)
    {
        services.AddOptions<OptiPowerToolHangfireOptions>()
            .Configure<IConfiguration>((options, configuration) =>
            {
                configuration.GetSection("OptiPowerTools:Hangfire").Bind(options);
                setupAction(options);
            });

        services.AddHangfire((serviceProvider, config) =>
        {
            var options = serviceProvider.GetRequiredService<IOptions<OptiPowerToolHangfireOptions>>().Value;

            config
                .SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UseSqlServerStorage(options.ConnectionString, new SqlServerStorageOptions
                {
                    SchemaName = options.SchemaName
                });

            if (options.EnableConsole)
                config.UseConsole();
        });

        services.AddHangfireServer();

        services.AddSingleton<OptimizelyDashboardAuthorizationFilter>();
        services.AddSingleton<HangfireMenuProvider>();
    }
}
