using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Tests.Configuration;

public class OptiPowerToolHangfireOptionsTests
{
    [Fact]
    public void DefaultOptions_HasExpectedDefaults()
    {
        // Arrange & Act
        var options = new OptiPowerToolHangfireOptions();

        // Assert
        Assert.Equal(string.Empty, options.ConnectionString);
        Assert.Equal("/episerver/backoffice/Plugins/hangfire", options.DashboardPath);
        Assert.Equal("OptiPowerTools Hangfire Dashboard", options.DashboardTitle);
        Assert.Equal("hangfire", options.SchemaName);
    }

    [Fact]
    public void DefaultOptions_HasExpectedToggleDefaults()
    {
        // Arrange & Act
        var options = new OptiPowerToolHangfireOptions();

        // Assert
        Assert.True(options.EnableDashboard);
        Assert.True(options.EnableConsole);
        Assert.True(options.EnableCmsMenu);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Options_CanSetEnableDashboard(bool value)
    {
        var options = new OptiPowerToolHangfireOptions { EnableDashboard = value };
        Assert.Equal(value, options.EnableDashboard);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Options_CanSetEnableConsole(bool value)
    {
        var options = new OptiPowerToolHangfireOptions { EnableConsole = value };
        Assert.Equal(value, options.EnableConsole);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Options_CanSetEnableCmsMenu(bool value)
    {
        var options = new OptiPowerToolHangfireOptions { EnableCmsMenu = value };
        Assert.Equal(value, options.EnableCmsMenu);
    }

    [Fact]
    public void DefaultOptions_HasExpectedAuthorizedRoles()
    {
        // Arrange & Act
        var options = new OptiPowerToolHangfireOptions();

        // Assert
        Assert.Equal(3, options.AuthorizedRoles.Length);
        Assert.Contains("Administrators", options.AuthorizedRoles);
        Assert.Contains("CmsAdmins", options.AuthorizedRoles);
        Assert.Contains("WebAdmins", options.AuthorizedRoles);
    }

    [Fact]
    public void Options_CanSetConnectionString()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();
        var expected = "Server=myserver;Database=mydb;Trusted_Connection=True;";

        // Act
        options.ConnectionString = expected;

        // Assert
        Assert.Equal(expected, options.ConnectionString);
    }

    [Fact]
    public void Options_CanSetDashboardPath()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();

        // Act
        options.DashboardPath = "/custom-hangfire";

        // Assert
        Assert.Equal("/custom-hangfire", options.DashboardPath);
    }

    [Fact]
    public void Options_CanOverrideAuthorizedRoles()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();
        var customRoles = new[] { "SuperAdmin" };

        // Act
        options.AuthorizedRoles = customRoles;

        // Assert
        Assert.Single(options.AuthorizedRoles);
        Assert.Equal("SuperAdmin", options.AuthorizedRoles[0]);
    }
}
