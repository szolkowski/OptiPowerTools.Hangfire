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
        Assert.True(options.EnableStandardAuthorization);
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

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Options_CanSetEnableStandardAuthorization(bool value)
    {
        var options = new OptiPowerToolHangfireOptions { EnableStandardAuthorization = value };
        Assert.Equal(value, options.EnableStandardAuthorization);
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
        var options = new OptiPowerToolHangfireOptions
        {
            // Act
            DashboardPath = "/custom-hangfire"
        };

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

    [Fact]
    public void DefaultOptions_MenuPlacement_IsCmsSection()
    {
        var options = new OptiPowerToolHangfireOptions();
        Assert.Equal(CmsMenuPlacement.CmsSection, options.MenuPlacement);
    }

    [Fact]
    public void DefaultOptions_MenuPath_IsNull()
    {
        var options = new OptiPowerToolHangfireOptions();
        Assert.Null(options.MenuPath);
    }

    [Fact]
    public void DefaultOptions_MenuSortIndex_IsNull()
    {
        var options = new OptiPowerToolHangfireOptions();
        Assert.Null(options.MenuSortIndex);
    }

    [Fact]
    public void DefaultOptions_CustomSectionName_IsOptiPowerTools()
    {
        var options = new OptiPowerToolHangfireOptions();
        Assert.Equal("OptiPowerTools", options.CustomSectionName);
    }

    [Theory]
    [InlineData(CmsMenuPlacement.CmsSection)]
    [InlineData(CmsMenuPlacement.TopLevel)]
    [InlineData(CmsMenuPlacement.CustomSection)]
    public void Options_CanSetMenuPlacement(CmsMenuPlacement value)
    {
        var options = new OptiPowerToolHangfireOptions { MenuPlacement = value };
        Assert.Equal(value, options.MenuPlacement);
    }

    [Fact]
    public void Options_CanSetMenuPath()
    {
        var options = new OptiPowerToolHangfireOptions { MenuPath = "/global/custom/path" };
        Assert.Equal("/global/custom/path", options.MenuPath);
    }

    [Fact]
    public void Options_CanSetMenuSortIndex()
    {
        var options = new OptiPowerToolHangfireOptions { MenuSortIndex = 42 };
        Assert.Equal(42, options.MenuSortIndex);
    }

    [Fact]
    public void Options_CanSetCustomSectionName()
    {
        var options = new OptiPowerToolHangfireOptions { CustomSectionName = "My Section" };
        Assert.Equal("My Section", options.CustomSectionName);
    }

    [Fact]
    public void DefaultOptions_JobExpirationCheckInterval_Is15Minutes()
    {
        var options = new OptiPowerToolHangfireOptions();
        Assert.Equal(TimeSpan.FromMinutes(15), options.JobExpirationCheckInterval);
    }

    [Fact]
    public void Options_CanSetJobExpirationCheckInterval()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            JobExpirationCheckInterval = TimeSpan.FromMinutes(30)
        };
        Assert.Equal(TimeSpan.FromMinutes(30), options.JobExpirationCheckInterval);
    }
}
