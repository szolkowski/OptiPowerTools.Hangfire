using EPiServer.Shell.Navigation;
using Microsoft.Extensions.Options;
using NSubstitute;
using OptiPowerTools.Hangfire.Cms;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Tests.Cms;

public class HangfireMenuProviderTests
{
    private static HangfireMenuProvider CreateProvider(
        OptiPowerToolHangfireOptions? optiOptions = null)
    {
        var opts = optiOptions ?? new OptiPowerToolHangfireOptions();
        var options = Substitute.For<IOptions<OptiPowerToolHangfireOptions>>();
        options.Value.Returns(opts);
        return new HangfireMenuProvider(options);
    }

    [Fact]
    public void GetMenuItems_WhenEnabled_ReturnsSingleMenuItem()
    {
        var provider = CreateProvider();

        var items = provider.GetMenuItems().ToList();

        Assert.Single(items);
    }

    [Fact]
    public void GetMenuItems_WhenEnabled_ReturnsUrlMenuItem()
    {
        var provider = CreateProvider();

        var item = provider.GetMenuItems().Single();

        var urlItem = Assert.IsType<UrlMenuItem>(item);
        Assert.Equal("/HangfireCms/Index", urlItem.Url);
    }

    [Fact]
    public void GetMenuItems_WhenEnabled_UsesCorrectMenuPath()
    {
        var provider = CreateProvider();

        var item = provider.GetMenuItems().Single();

        Assert.Equal(MenuPaths.Global + "/cms/hangfire", item.Path);
    }

    [Fact]
    public void GetMenuItems_WhenEnabled_UsesDashboardTitleFromOptions()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            DashboardTitle = "My Custom Title"
        };
        var provider = CreateProvider(options);

        var item = provider.GetMenuItems().Single();

        Assert.Equal("My Custom Title", item.Text);
    }

    [Fact]
    public void GetMenuItems_WhenDisabled_ReturnsEmpty()
    {
        var options = new OptiPowerToolHangfireOptions { EnableCmsMenu = false };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems();

        Assert.Empty(items);
    }

    [Fact]
    public void GetMenuItems_WhenEnabled_SetsSortIndex()
    {
        var provider = CreateProvider();

        var item = provider.GetMenuItems().Single();

        Assert.Equal(SortIndex.Last - 10, item.SortIndex);
    }
}
