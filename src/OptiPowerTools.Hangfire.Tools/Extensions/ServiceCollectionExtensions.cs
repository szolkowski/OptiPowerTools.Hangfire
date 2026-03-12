using Microsoft.Extensions.DependencyInjection;

namespace OptiPowerTools.Hangfire.Tools.Extensions;

/// <summary>
/// Extension methods for configuring OptiPowerTools Hangfire Tools services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds OptiPowerTools Hangfire Tools services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    public static IServiceCollection AddOptiPowerToolHangfireTools(this IServiceCollection services)
    {
        return services;
    }
}
