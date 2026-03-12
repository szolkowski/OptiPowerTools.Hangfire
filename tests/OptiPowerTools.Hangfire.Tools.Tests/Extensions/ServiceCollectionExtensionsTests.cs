using Microsoft.Extensions.DependencyInjection;
using OptiPowerTools.Hangfire.Tools.Extensions;

namespace OptiPowerTools.Hangfire.Tools.Tests.Extensions;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddOptiPowerToolHangfireTools_ReturnsServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();

        // Act
        var result = services.AddOptiPowerToolHangfireTools();

        // Assert
        Assert.Same(services, result);
    }
}
