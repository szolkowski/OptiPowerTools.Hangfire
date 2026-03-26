using Microsoft.AspNetCore.Mvc.ApplicationModels;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Cms;

/// <summary>
/// Application model convention that sets the CMS shell controller route
/// from <see cref="OptiPowerToolHangfireOptions.CmsShellPath"/> at startup.
/// </summary>
internal sealed class HangfireCmsRouteConvention : IApplicationModelConvention
{
    private readonly string _path;

    public HangfireCmsRouteConvention(string path) => _path = path;

    public void Apply(ApplicationModel application)
    {
        var controller = application.Controllers
            .FirstOrDefault(c => c.ControllerType == typeof(HangfireCmsController));

        if (controller is null)
            return;

        var action = controller.Actions.FirstOrDefault(a => a.ActionName == nameof(HangfireCmsController.Index));

        if (action is null)
            return;

        action.Selectors.Clear();
        action.Selectors.Add(new SelectorModel
        {
            AttributeRouteModel = new AttributeRouteModel
            {
                Template = _path
            }
        });
    }
}
