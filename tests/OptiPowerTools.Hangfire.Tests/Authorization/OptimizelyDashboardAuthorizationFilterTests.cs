using System.Security.Claims;
using Microsoft.Extensions.Options;
using NSubstitute;
using OptiPowerTools.Hangfire.Authorization;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Tests.Authorization;

public class OptimizelyDashboardAuthorizationFilterTests
{
    private static OptimizelyDashboardAuthorizationFilter CreateFilter(
        string[]? authorizedRoles = null)
    {
        var hangfireOptions = new OptiPowerToolHangfireOptions
        {
            AuthorizedRoles = authorizedRoles ?? ["Administrators", "CmsAdmins", "WebAdmins"]
        };
        var options = Substitute.For<IOptions<OptiPowerToolHangfireOptions>>();
        options.Value.Returns(hangfireOptions);
        return new OptimizelyDashboardAuthorizationFilter(options);
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
    public void Constructor_WithOptions_DoesNotThrow()
    {
        var filter = CreateFilter();
        Assert.NotNull(filter);
    }

    [Fact]
    public void Constructor_WithCustomRoles_DoesNotThrow()
    {
        var filter = CreateFilter(["CustomRole"]);
        Assert.NotNull(filter);
    }

    [Fact]
    public void IsAuthorized_UnauthenticatedUser_ReturnsFalse()
    {
        var filter = CreateFilter();
        var user = CreatePrincipal(isAuthenticated: false);

        Assert.False(filter.IsAuthorized(user));
    }

    [Fact]
    public void IsAuthorized_AuthenticatedUserWithNoRoles_ReturnsFalse()
    {
        var filter = CreateFilter();
        var user = CreatePrincipal(isAuthenticated: true);

        Assert.False(filter.IsAuthorized(user));
    }

    [Fact]
    public void IsAuthorized_AuthenticatedUserWithWrongRole_ReturnsFalse()
    {
        var filter = CreateFilter(["Administrators"]);
        var user = CreatePrincipal(isAuthenticated: true, "Editors");

        Assert.False(filter.IsAuthorized(user));
    }

    [Theory]
    [InlineData("Administrators")]
    [InlineData("CmsAdmins")]
    [InlineData("WebAdmins")]
    public void IsAuthorized_AuthenticatedUserWithDefaultRole_ReturnsTrue(string role)
    {
        var filter = CreateFilter();
        var user = CreatePrincipal(isAuthenticated: true, role);

        Assert.True(filter.IsAuthorized(user));
    }

    [Fact]
    public void IsAuthorized_AuthenticatedUserWithCustomRole_ReturnsTrue()
    {
        var filter = CreateFilter(["SuperAdmin"]);
        var user = CreatePrincipal(isAuthenticated: true, "SuperAdmin");

        Assert.True(filter.IsAuthorized(user));
    }

    [Fact]
    public void IsAuthorized_AuthenticatedUserWithOneOfMultipleRoles_ReturnsTrue()
    {
        var filter = CreateFilter(["Admin", "Editor", "Viewer"]);
        var user = CreatePrincipal(isAuthenticated: true, "Editor");

        Assert.True(filter.IsAuthorized(user));
    }

    [Fact]
    public void IsAuthorized_EmptyAuthorizedRoles_ReturnsFalse()
    {
        var filter = CreateFilter(Array.Empty<string>());
        var user = CreatePrincipal(isAuthenticated: true, "Administrators");

        Assert.False(filter.IsAuthorized(user));
    }
}
