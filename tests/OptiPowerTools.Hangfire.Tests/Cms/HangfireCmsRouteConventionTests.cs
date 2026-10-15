using System.Reflection;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OptiPowerTools.Hangfire.Cms;

namespace OptiPowerTools.Hangfire.Tests.Cms;

public class HangfireCmsRouteConventionTests
{
    private const string _path = "/custom/hangfire-shell";

    private class OtherController : Controller
    {
        public OkResult Index() => Ok();
    }

    private static ControllerModel CreateControllerModel<TController>(params string[] actionNames)
    {
        var controller = new ControllerModel(typeof(TController).GetTypeInfo(), []);

        foreach (var actionName in actionNames)
        {
            var method = typeof(TController).GetMethod(actionName)!;
            var action = new ActionModel(method, []) { ActionName = actionName, Controller = controller };
            action.Selectors.Add(new SelectorModel
            {
                AttributeRouteModel = new AttributeRouteModel { Template = "original" }
            });
            controller.Actions.Add(action);
        }

        return controller;
    }

    private static ApplicationModel CreateApplication(params ControllerModel[] controllers)
    {
        var application = new ApplicationModel();
        foreach (var controller in controllers)
            application.Controllers.Add(controller);
        return application;
    }

    [Fact]
    public void Apply_HangfireCmsControllerIndex_ReplacesSelectorsWithConfiguredRoute()
    {
        // Arrange
        var other = CreateControllerModel<OtherController>(nameof(OtherController.Index));
        var hangfire = CreateControllerModel<HangfireCmsController>(nameof(HangfireCmsController.Index));
        var application = CreateApplication(other, hangfire);
        var convention = new HangfireCmsRouteConvention(_path);

        // Act
        convention.Apply(application);

        // Assert
        var selector = Assert.Single(hangfire.Actions[0].Selectors);
        Assert.Equal(_path, selector.AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_OtherControllers_AreNotModified()
    {
        // Arrange
        var other = CreateControllerModel<OtherController>(nameof(OtherController.Index));
        var hangfire = CreateControllerModel<HangfireCmsController>(nameof(HangfireCmsController.Index));
        var application = CreateApplication(other, hangfire);
        var convention = new HangfireCmsRouteConvention(_path);

        // Act
        convention.Apply(application);

        // Assert
        var selector = Assert.Single(other.Actions[0].Selectors);
        Assert.Equal("original", selector.AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_NoHangfireCmsController_DoesNotThrow()
    {
        // Arrange
        var other = CreateControllerModel<OtherController>(nameof(OtherController.Index));
        var application = CreateApplication(other);
        var convention = new HangfireCmsRouteConvention(_path);

        // Act
        var exception = Record.Exception(() => convention.Apply(application));

        // Assert
        Assert.Null(exception);
        Assert.Equal("original", Assert.Single(other.Actions[0].Selectors).AttributeRouteModel?.Template);
    }

    [Fact]
    public void Apply_HangfireCmsControllerWithoutIndexAction_DoesNotThrow()
    {
        // Arrange
        var hangfire = CreateControllerModel<HangfireCmsController>();
        var application = CreateApplication(hangfire);
        var convention = new HangfireCmsRouteConvention(_path);

        // Act
        var exception = Record.Exception(() => convention.Apply(application));

        // Assert
        Assert.Null(exception);
    }
}
