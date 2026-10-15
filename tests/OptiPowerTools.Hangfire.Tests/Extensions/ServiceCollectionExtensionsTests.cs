using Hangfire;
using Hangfire.Dashboard;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Authorization;
using OptiPowerTools.Hangfire.Cms;
using OptiPowerTools.Hangfire.Configuration;
using OptiPowerTools.Hangfire.Extensions;

namespace OptiPowerTools.Hangfire.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOptiPowerToolHangfire_WithDefaults_RegistersOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetService<IOptions<OptiPowerToolHangfireOptions>>();
        Assert.NotNull(options);
        Assert.NotNull(options.Value);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_WithAction_ConfiguresConnectionString()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        var expectedConnectionString = "Server=.;Database=HangfireTest;Trusted_Connection=True;";

        // Act
        services.AddOptiPowerToolHangfire(options =>
        {
            options.ConnectionString = expectedConnectionString;
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OptiPowerToolHangfireOptions>>();
        Assert.Equal(expectedConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_ReturnsServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOptiPowerToolHangfire();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_RegistersAuthFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(OptimizelyDashboardAuthorizationFilter));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_RegistersHangfireServer()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IHostedService));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_RegistersMenuProvider()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(HangfireMenuProvider));
        Assert.NotNull(descriptor);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_WithConfiguration_BindsFromConfig()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OptiPowerTools:Hangfire:SchemaName"] = "custom_schema",
                ["OptiPowerTools:Hangfire:DashboardTitle"] = "Custom Title"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OptiPowerToolHangfireOptions>>().Value;
        Assert.Equal("custom_schema", options.SchemaName);
        Assert.Equal("Custom Title", options.DashboardTitle);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_ActionOverridesConfiguration()
    {
        // Arrange
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["OptiPowerTools:Hangfire:SchemaName"] = "config_schema"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(config);
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire(options =>
        {
            options.SchemaName = "action_schema";
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OptiPowerToolHangfireOptions>>().Value;
        Assert.Equal("action_schema", options.SchemaName);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_NonGeneric_DoesNotRegisterIDashboardAuthorizationFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IDashboardAuthorizationFilter));
        Assert.Null(descriptor);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_Generic_RegistersCustomFilterAsIDashboardAuthorizationFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire<StubAuthorizationFilter>();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(IDashboardAuthorizationFilter));
        Assert.NotNull(descriptor);
        Assert.Equal(typeof(StubAuthorizationFilter), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_Generic_StillRegistersStandardFilter()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();

        // Act
        services.AddOptiPowerToolHangfire<StubAuthorizationFilter>();

        // Assert
        var descriptor = services.FirstOrDefault(d =>
            d.ServiceType == typeof(OptimizelyDashboardAuthorizationFilter));
        Assert.NotNull(descriptor);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_Generic_WithAction_ConfiguresOptions()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        var expectedConnectionString = "Server=.;Database=Test;";

        // Act
        services.AddOptiPowerToolHangfire<StubAuthorizationFilter>(options =>
        {
            options.ConnectionString = expectedConnectionString;
        });

        // Assert
        var provider = services.BuildServiceProvider();
        var options = provider.GetRequiredService<IOptions<OptiPowerToolHangfireOptions>>();
        Assert.Equal(expectedConnectionString, options.Value.ConnectionString);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_Generic_ReturnsServiceCollection_ForChaining()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOptiPowerToolHangfire<StubAuthorizationFilter>();

        // Assert
        Assert.Same(services, result);
    }

    [Fact]
    public void AddOptiPowerToolHangfire_RegistersHttpContextAccessor()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(IHttpContextAccessor));
    }

    [Fact]
    public void AddOptiPowerToolHangfire_RegistersHangfireConfiguration()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        Assert.Contains(services, d => d.ServiceType == typeof(IGlobalConfiguration));
    }

    [Fact]
    public void AddOptiPowerToolHangfire_RegistersMvcRouteConventionOptions()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        services.AddOptiPowerToolHangfire();

        // Assert
        var descriptor = Assert.Single(services, d =>
            d.ServiceType == typeof(IConfigureOptions<MvcOptions>)
            && d.ImplementationType == typeof(ConfigureHangfireMvcOptions));
        Assert.Equal(ServiceLifetime.Singleton, descriptor.Lifetime);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void AddOptiPowerToolHangfire_MissingConnectionString_ThrowsWhenHangfireIsConfigured(string? connectionString)
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
        services.AddLogging();
        services.AddOptiPowerToolHangfire(options => options.ConnectionString = connectionString!);
        var provider = services.BuildServiceProvider();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(() =>
            provider.GetRequiredService<IGlobalConfiguration>());

        // Assert
        Assert.Equal(
            "A connection string must be configured for Hangfire SQL Server storage. " +
            "Set 'OptiPowerTools:Hangfire:ConnectionString' in configuration or pass it via the setup action.",
            exception.Message);
    }

    private class StubAuthorizationFilter : IDashboardAuthorizationFilter
    {
        public bool Authorize(DashboardContext context) => true;
    }
}
