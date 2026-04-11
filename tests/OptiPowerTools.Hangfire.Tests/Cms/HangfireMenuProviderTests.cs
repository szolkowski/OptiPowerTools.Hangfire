using EPiServer.Shell.Navigation;
using Microsoft.AspNetCore.Http;
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
        var httpContextAccessor = Substitute.For<IHttpContextAccessor>();
        return new HangfireMenuProvider(options, httpContextAccessor);
    }

    // === CmsSection (default, backward-compatible) ===

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

    // === CmsSection with overrides ===

    [Fact]
    public void GetMenuItems_CmsSection_WithMenuPathOverride_UsesCustomPath()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPath = "/admin/hangfire"
        };
        var provider = CreateProvider(options);

        var item = provider.GetMenuItems().Single();

        Assert.Equal(MenuPaths.Global + "/admin/hangfire", item.Path);
    }

    [Fact]
    public void GetMenuItems_CmsSection_WithSortIndexOverride_UsesCustomSortIndex()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuSortIndex = 500
        };
        var provider = CreateProvider(options);

        var item = provider.GetMenuItems().Single();

        Assert.Equal(500, item.SortIndex);
    }

    // === TopLevel ===

    [Fact]
    public void GetMenuItems_TopLevel_ReturnsTwoMenuItems()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void GetMenuItems_TopLevel_FirstItemIsSectionMenuItem()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.IsType<SectionMenuItem>(items[0]);
    }

    [Fact]
    public void GetMenuItems_TopLevel_SecondItemIsUrlMenuItem()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        var urlItem = Assert.IsType<UrlMenuItem>(items[1]);
        Assert.Equal("/HangfireCms/Index", urlItem.Url);
    }

    [Fact]
    public void GetMenuItems_TopLevel_SectionPathDerivedFromCustomSectionName()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/optipowertools", items[0].Path);
        Assert.Equal(MenuPaths.Global + "/optipowertools/hangfire", items[1].Path);
    }

    [Fact]
    public void GetMenuItems_TopLevel_UsesDashboardTitleFromOptions()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel,
            DashboardTitle = "Custom Title"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("Custom Title", items[1].Text);
    }

    [Fact]
    public void GetMenuItems_TopLevel_DefaultSortIndex()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(SortIndex.Last - 10, items[0].SortIndex);
        Assert.Equal(SortIndex.Last - 10, items[1].SortIndex);
    }

    [Fact]
    public void GetMenuItems_TopLevel_WithMenuPathOverride_UsesCustomPath()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel,
            MenuPath = "/tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/tools", items[0].Path);
        Assert.Equal(MenuPaths.Global + "/tools/hangfire", items[1].Path);
    }

    [Fact]
    public void GetMenuItems_TopLevel_WithSortIndexOverride_UsesCustomSortIndex()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel,
            MenuSortIndex = 200
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(200, items[0].SortIndex);
        Assert.Equal(200, items[1].SortIndex);
    }

    // === CustomSection ===

    [Fact]
    public void GetMenuItems_CustomSection_ReturnsTwoMenuItems()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(2, items.Count);
    }

    [Fact]
    public void GetMenuItems_CustomSection_FirstItemIsSectionMenuItem()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.IsType<SectionMenuItem>(items[0]);
    }

    [Fact]
    public void GetMenuItems_CustomSection_SecondItemIsUrlMenuItem()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        var urlItem = Assert.IsType<UrlMenuItem>(items[1]);
        Assert.Equal("/HangfireCms/Index", urlItem.Url);
    }

    [Fact]
    public void GetMenuItems_CustomSection_SectionUsesCustomSectionName()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            CustomSectionName = "My Tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("My Tools", items[0].Text);
    }

    [Fact]
    public void GetMenuItems_CustomSection_SectionPathDerivedFromName()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            CustomSectionName = "My Tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/my-tools", items[0].Path);
    }

    [Fact]
    public void GetMenuItems_CustomSection_ItemPathNested()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            CustomSectionName = "My Tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/my-tools/hangfire", items[1].Path);
    }

    [Fact]
    public void GetMenuItems_CustomSection_DefaultSectionName_IsOptiPowerTools()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("OptiPowerTools", items[0].Text);
        Assert.Equal(MenuPaths.Global + "/optipowertools", items[0].Path);
    }

    [Fact]
    public void GetMenuItems_CustomSection_ItemSortIndex_Is100()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(100, items[1].SortIndex);
    }

    [Fact]
    public void GetMenuItems_CustomSection_SectionSortIndex_DefaultsToLastMinus10()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(SortIndex.Last - 10, items[0].SortIndex);
    }

    [Fact]
    public void GetMenuItems_CustomSection_WithMenuPathOverride_OverridesSectionPath()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            MenuPath = "/custom"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/custom", items[0].Path);
        Assert.Equal(MenuPaths.Global + "/custom/hangfire", items[1].Path);
    }

    [Fact]
    public void GetMenuItems_CustomSection_WithSortIndexOverride_OverridesSectionSortIndex()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            MenuSortIndex = 3000
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(3000, items[0].SortIndex);
        Assert.Equal(100, items[1].SortIndex);
    }

    [Fact]
    public void GetMenuItems_CustomSection_UsesDashboardTitleForItem()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            DashboardTitle = "Jobs"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("Jobs", items[1].Text);
    }

    // === CustomSection slug edge cases ===

    [Theory]
    [InlineData("My Tools & Utils", "my-tools-utils")]
    [InlineData("Tools (Beta)", "tools-beta")]
    [InlineData("  Spaced  ", "spaced")]
    [InlineData("Dots.And_Underscores", "dots-and-underscores")]
    public void GetMenuItems_CustomSection_SpecialCharactersInName_ProducesValidSlug(
        string sectionName, string expectedSlug)
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            CustomSectionName = sectionName
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/" + expectedSlug, items[0].Path);
    }

    // === CustomMenuItemName ===

    [Fact]
    public void GetMenuItems_CmsSection_WithCustomMenuItemName_UsesCustomName()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            CustomMenuItemName = "Background Jobs"
        };
        var provider = CreateProvider(options);

        var item = provider.GetMenuItems().Single();

        Assert.Equal("Background Jobs", item.Text);
    }

    [Fact]
    public void GetMenuItems_CmsSection_EmptyCustomMenuItemName_FallsBackToDashboardTitle()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            CustomMenuItemName = "",
            DashboardTitle = "My Dashboard"
        };
        var provider = CreateProvider(options);

        var item = provider.GetMenuItems().Single();

        Assert.Equal("My Dashboard", item.Text);
    }

    [Fact]
    public void GetMenuItems_CustomSection_WithCustomMenuItemName_UsesCustomNameForItem()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.CustomSection,
            CustomMenuItemName = "Jobs"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("Jobs", items[1].Text);
    }

    // === MenuPath normalization ===

    [Fact]
    public void GetMenuItems_CmsSection_MenuPathWithoutLeadingSlash_NormalizesPath()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPath = "admin/hangfire"
        };
        var provider = CreateProvider(options);

        var item = provider.GetMenuItems().Single();

        Assert.Equal(MenuPaths.Global + "/admin/hangfire", item.Path);
    }

    [Fact]
    public void GetMenuItems_TopLevel_MenuPathWithoutLeadingSlash_NormalizesPath()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel,
            MenuPath = "tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/tools", items[0].Path);
        Assert.Equal(MenuPaths.Global + "/tools/hangfire", items[1].Path);
    }

    // === TopLevel CustomSectionName ===

    [Fact]
    public void GetMenuItems_TopLevel_WithCustomSectionName_UsesSectionNameText()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel,
            CustomSectionName = "My Tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("My Tools", items[0].Text);
    }

    [Fact]
    public void GetMenuItems_TopLevel_WithCustomSectionName_DerivesSectionSlug()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel,
            CustomSectionName = "My Tools"
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal(MenuPaths.Global + "/my-tools", items[0].Path);
        Assert.Equal(MenuPaths.Global + "/my-tools/hangfire", items[1].Path);
    }

    [Fact]
    public void GetMenuItems_TopLevel_DefaultSectionNameText_IsOptiPowerTools()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems().ToList();

        Assert.Equal("OptiPowerTools", items[0].Text);
    }

    // === Disabled with non-default placement ===

    [Fact]
    public void GetMenuItems_WhenDisabled_ReturnsEmpty_RegardlessOfPlacement()
    {
        var options = new OptiPowerToolHangfireOptions
        {
            EnableCmsMenu = false,
            MenuPlacement = CmsMenuPlacement.TopLevel
        };
        var provider = CreateProvider(options);

        var items = provider.GetMenuItems();

        Assert.Empty(items);
    }
}
