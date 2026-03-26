using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Cms;

/// <summary>
/// Configures MVC options to register the <see cref="HangfireCmsRouteConvention"/>
/// using the resolved <see cref="OptiPowerToolHangfireOptions.CmsShellPath"/>.
/// </summary>
internal sealed class ConfigureHangfireMvcOptions : IConfigureOptions<MvcOptions>
{
    private readonly OptiPowerToolHangfireOptions _options;

    public ConfigureHangfireMvcOptions(IOptions<OptiPowerToolHangfireOptions> options) =>
        _options = options.Value;

    public void Configure(MvcOptions mvcOptions) =>
        mvcOptions.Conventions.Add(new HangfireCmsRouteConvention(_options.CmsShellPath));
}
