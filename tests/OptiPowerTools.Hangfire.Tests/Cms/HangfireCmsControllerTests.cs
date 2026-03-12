using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using NSubstitute;
using OptiPowerTools.Hangfire.Cms;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Tests.Cms;

public class HangfireCmsControllerTests
{
    private static HangfireCmsController CreateController(
        OptiPowerToolHangfireOptions optiOptions,
        ClaimsPrincipal user)
    {
        var options = Substitute.For<IOptions<OptiPowerToolHangfireOptions>>();
        options.Value.Returns(optiOptions);

        return new HangfireCmsController(options)
        {
            ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            }
        };
    }

    private static ClaimsPrincipal CreatePrincipal(
        bool isAuthenticated, params string[] roles)
    {
        var claims = new List<Claim>();
        if (isAuthenticated)
            claims.Add(new Claim(ClaimTypes.Name, "testuser"));

        foreach (var role in roles)
            claims.Add(new Claim(ClaimTypes.Role, role));

        var identity = new ClaimsIdentity(
            claims,
            isAuthenticated ? "TestAuth" : null);
        return new ClaimsPrincipal(identity);
    }

    [Fact]
    public void Index_AuthorizedUser_ReturnsViewResult()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();
        var user = CreatePrincipal(true, "Administrators");
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Equal(options.DashboardPath, controller.ViewBag.DashboardPath);
        Assert.Equal(options.DashboardTitle, controller.ViewBag.DashboardTitle);
    }

    [Fact]
    public void Index_UnauthorizedUser_ReturnsForbid()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();
        var user = CreatePrincipal(true, "Editors");
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void Index_UnauthenticatedUser_ReturnsForbid()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();
        var user = CreatePrincipal(false);
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void Index_CustomDashboardPath_SetsViewBagCorrectly()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions
        {
            DashboardPath = "/custom/hangfire",
            DashboardTitle = "My Jobs Dashboard"
        };
        var user = CreatePrincipal(true, "Administrators");
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
        Assert.Equal("/custom/hangfire", controller.ViewBag.DashboardPath);
        Assert.Equal("My Jobs Dashboard", controller.ViewBag.DashboardTitle);
    }

    [Theory]
    [InlineData("Administrators")]
    [InlineData("CmsAdmins")]
    [InlineData("WebAdmins")]
    public void Index_UserInAnyDefaultRole_ReturnsViewResult(string role)
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions();
        var user = CreatePrincipal(true, role);
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Index_UserWithCustomAuthorizedRole_ReturnsViewResult()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions
        {
            AuthorizedRoles = ["SuperAdmin"]
        };
        var user = CreatePrincipal(true, "SuperAdmin");
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Index_EmptyAuthorizedRoles_ReturnsForbid()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions
        {
            AuthorizedRoles = []
        };
        var user = CreatePrincipal(true, "Administrators");
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ForbidResult>(result);
    }

    [Fact]
    public void Index_NullAuthorizedRoles_ReturnsForbid()
    {
        // Arrange
        var options = new OptiPowerToolHangfireOptions
        {
            AuthorizedRoles = null!
        };
        var user = CreatePrincipal(true, "Administrators");
        var controller = CreateController(options, user);

        // Act
        var result = controller.Index();

        // Assert
        Assert.IsType<ForbidResult>(result);
    }
}
