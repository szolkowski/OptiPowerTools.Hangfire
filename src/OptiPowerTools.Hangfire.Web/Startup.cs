using Hangfire;
using OptiPowerTools.Hangfire.Extensions;
using OptiPowerTools.Hangfire.Web.Jobs;
using OptiPowerTools.Hangfire.Web.Samples;

namespace OptiPowerTools.Hangfire.Web;

public class Startup
{
    private readonly Foundation.Startup _foundationStartup;
    private readonly IConfiguration _configuration;

    public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
    {
        _foundationStartup = new Foundation.Startup(webHostingEnvironment, configuration);
        _configuration = configuration;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        _foundationStartup.ConfigureServices(services);

        services.AddOptiPowerToolHangfire(options =>
        {
            options.ConnectionString = _configuration.GetConnectionString("EPiServerDB") ?? throw new InvalidOperationException("Hangfire connection string is not configured.");
        });
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

        RecurringJob.AddOrUpdate<ConsoleShowcaseJob>("console-showcase", j => j.Execute(null!), Cron.Hourly);
        RecurringJob.AddOrUpdate<OrderPipelineJob>("order-pipeline", j => j.Start(null!), Cron.Hourly);
        RecurringJob.AddOrUpdate<ScheduledCleanupJob>("scheduled-cleanup", j => j.Plan(null!), Cron.Daily);
        RecurringJob.AddOrUpdate<CancellableExportJob>("cancellable-export", j => j.Execute(null!, null!), Cron.Daily);
    }
}
