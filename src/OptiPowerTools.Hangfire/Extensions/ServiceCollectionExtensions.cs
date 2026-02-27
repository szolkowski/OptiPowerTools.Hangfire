using Hangfire;
using Hangfire.Console;
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
    /// Connection string must be provided via appsettings.json under "OptiPowerTools:Hangfire:ConnectionString".
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiPowerToolHangfire(this IServiceCollection services)
    {
        return services.AddOptiPowerToolHangfire(_ => { });
    }

    /// <summary>
    /// Adds Hangfire services configured for Optimizely CMS with the specified options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="setupAction">An action to configure <see cref="OptiPowerToolHangfireOptions"/>.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddOptiPowerToolHangfire(
        this IServiceCollection services,
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

        return services;
    }
}
