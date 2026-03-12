using Hangfire.Dashboard;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NSubstitute;
using OptiPowerTools.Hangfire.Authorization;
using OptiPowerTools.Hangfire.Configuration;
using HangfireAppBuilderExtensions = OptiPowerTools.Hangfire.Extensions.ApplicationBuilderExtensions;

namespace OptiPowerTools.Hangfire.Tests.Extensions;

public class ApplicationBuilderExtensionsTests
{
    private static IServiceProvider CreateServiceProvider(
        OptiPowerToolHangfireOptions? options = null,
        IDashboardAuthorizationFilter? customFilter = null)
    {
        options ??= new OptiPowerToolHangfireOptions();

        var optionsWrapper = Substitute.For<IOptions<OptiPowerToolHangfireOptions>>();
        optionsWrapper.Value.Returns(options);

        var services = new ServiceCollection();
        services.AddSingleton(optionsWrapper);
        services.AddSingleton(new OptimizelyDashboardAuthorizationFilter(optionsWrapper));

        if (customFilter is not null)
            services.AddSingleton(customFilter);

        return services.BuildServiceProvider();
    }

    [Fact]
    public void ResolveAuthorizationFilters_CustomFilterRegistered_ReturnsCustomFilter()
    {
        // Arrange
        var customFilter = Substitute.For<IDashboardAuthorizationFilter>();
        var options = new OptiPowerToolHangfireOptions { EnableStandardAuthorization = true };
        var serviceProvider = CreateServiceProvider(options, customFilter);

        // Act
        var result = HangfireAppBuilderExtensions.ResolveAuthorizationFilters(serviceProvider, options);

        // Assert
        var filters = result.ToList();
        Assert.Single(filters);
        Assert.Same(customFilter, filters[0]);
    }

    [Fact]
    public void ResolveAuthorizationFilters_CustomFilterRegistered_StandardAuthDisabled_ReturnsCustomFilter()
    {
        // Arrange
        var customFilter = Substitute.For<IDashboardAuthorizationFilter>();
        var options = new OptiPowerToolHangfireOptions { EnableStandardAuthorization = false };
        var serviceProvider = CreateServiceProvider(options, customFilter);

        // Act
        var result = HangfireAppBuilderExtensions.ResolveAuthorizationFilters(serviceProvider, options);

        // Assert
        var filters = result.ToList();
        Assert.Single(filters);
        Assert.Same(customFilter, filters[0]);
    }

    [Fact]
    public void ResolveAuthorizationFilters_NoCustomFilter_StandardAuthEnabled_ReturnsStandardFilter()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions { EnableStandardAuthorization = true };
        var serviceProvider = CreateServiceProvider(options);

        // Act
        var result = HangfireAppBuilderExtensions.ResolveAuthorizationFilters(serviceProvider, options);

        // Assert
        var filters = result.ToList();
        Assert.Single(filters);
        Assert.IsType<OptimizelyDashboardAuthorizationFilter>(filters[0]);
    }

    [Fact]
    public void ResolveAuthorizationFilters_NoCustomFilter_StandardAuthDisabled_ReturnsEmpty()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions { EnableStandardAuthorization = false };
        var serviceProvider = CreateServiceProvider(options);

        // Act
        var result = HangfireAppBuilderExtensions.ResolveAuthorizationFilters(serviceProvider, options);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public void UseOptiPowerToolHangfire_EnableDashboardFalse_ReturnsAppBuilder()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions { EnableDashboard = false };
        var serviceProvider = CreateServiceProvider(options);
        var app = Substitute.For<IApplicationBuilder>();
        app.ApplicationServices.Returns(serviceProvider);

        // Act
        var result = HangfireAppBuilderExtensions.UseOptiPowerToolHangfire(app);

        // Assert
        Assert.Same(app, result);
    }

    [Fact]
    public void UseOptiPowerToolHangfire_MissingOptions_ThrowsInvalidOperationException()
    {
        // Arrange
        var services = new ServiceCollection();
        var serviceProvider = services.BuildServiceProvider();
        var app = Substitute.For<IApplicationBuilder>();
        app.ApplicationServices.Returns(serviceProvider);

        // Act & Assert
        Assert.Throws<InvalidOperationException>(() => HangfireAppBuilderExtensions.UseOptiPowerToolHangfire(app));
    }
}
