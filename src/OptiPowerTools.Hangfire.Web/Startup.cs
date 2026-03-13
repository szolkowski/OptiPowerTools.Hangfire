using Hangfire;
using OptiPowerTools.Hangfire.Extensions;
using OptiPowerTools.Hangfire.Web.Jobs;

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

        RecurringJob.AddOrUpdate<DataImportJob>("data-import", j => j.Execute(null!), Cron.Minutely);
        RecurringJob.AddOrUpdate<DataExportJob>("data-export", j => j.Execute(null!), Cron.Minutely);
        RecurringJob.AddOrUpdate<ReportGeneratorJob>("report-gen", j => j.Execute(null!), Cron.Minutely);
        RecurringJob.AddOrUpdate<NotificationJob>("notification", j => j.Execute(null!), Cron.Minutely);
        RecurringJob.AddOrUpdate<MonthlyAuditJob>("monthly-audit", j => j.Execute(null!), Cron.Monthly);
    }
}
