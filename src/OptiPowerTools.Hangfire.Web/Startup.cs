using OptiPowerTools.Hangfire.Extensions;

namespace OptiPowerTools.Hangfire.Web;

public class Startup
{
    private readonly Foundation.Startup _foundationStartup;

    public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
    {
        _foundationStartup = new Foundation.Startup(webHostingEnvironment, configuration);
    }

    public void ConfigureServices(IServiceCollection services)
    {
        _foundationStartup.ConfigureServices(services);

        services.AddOptiPowerToolHangfire();
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        _foundationStartup.Configure(app, env);

        app.UseOptiPowerToolHangfire();
    }
}
