using EPiServer.Scheduler;
using Hangfire;
using OptiPowerTools.Hangfire.Extensions;
using OptiPowerTools.Hangfire.Web.Jobs;
using OptiPowerTools.Hangfire.Web.Samples;

namespace OptiPowerTools.Hangfire.Web;

public class Startup
{
    private readonly MyOptiAlloySite.Startup _alloySiteStartup;
    private readonly IConfiguration _configuration;
    private readonly IWebHostEnvironment _environment;

    public Startup(IWebHostEnvironment webHostingEnvironment, IConfiguration configuration)
    {
        _alloySiteStartup = new MyOptiAlloySite.Startup(webHostingEnvironment);
        _configuration = configuration;
        _environment = webHostingEnvironment;
    }

    public void ConfigureServices(IServiceCollection services)
    {
        // Ensure DataDirectory and scheduler config are set for non-Development environments
        // (MyOptiAlloySite.Startup only sets these in Development)
        if (!_environment.IsDevelopment())
        {
            AppDomain.CurrentDomain.SetData("DataDirectory", Path.Combine(_environment.ContentRootPath, "App_Data"));
            services.Configure<SchedulerOptions>(options => options.Enabled = false);
        }

        _alloySiteStartup.ConfigureServices(services);

        services.AddOptiPowerToolHangfire(options =>
        {
            options.ConnectionString = _configuration.GetConnectionString("EPiServerDB")
                ?? throw new InvalidOperationException("Hangfire connection string is not configured.");
        });
    }

    public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
    {
        _alloySiteStartup.Configure(app, env);

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
