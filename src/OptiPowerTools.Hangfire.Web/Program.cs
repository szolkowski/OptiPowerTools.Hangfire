using Hangfire;
using OptiPowerTools.Hangfire.Extensions;
using OptiPowerTools.Hangfire.Web.Jobs;
using OptiPowerTools.Hangfire.Web.Samples;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCms();

builder.Services.AddOptiPowerToolHangfire(options =>
{
    options.ConnectionString = builder.Configuration.GetConnectionString("EPiServerDB")
        ?? throw new InvalidOperationException("Hangfire connection string is not configured.");
});

var app = builder.Build();

app.UseAuthentication();
app.UseAuthorization();
app.UseOptiPowerToolHangfire();

app.MapContent();

RecurringJob.AddOrUpdate<DataImportJob>("data-import", j => j.Execute(null!), Cron.Minutely);
RecurringJob.AddOrUpdate<DataExportJob>("data-export", j => j.Execute(null!), Cron.Minutely);
RecurringJob.AddOrUpdate<ReportGeneratorJob>("report-gen", j => j.Execute(null!), Cron.Minutely);
RecurringJob.AddOrUpdate<NotificationJob>("notification", j => j.Execute(null!), Cron.Minutely);
RecurringJob.AddOrUpdate<MonthlyAuditJob>("monthly-audit", j => j.Execute(null!), Cron.Monthly);

RecurringJob.AddOrUpdate<ConsoleShowcaseJob>("console-showcase", j => j.Execute(null!), Cron.Hourly);
RecurringJob.AddOrUpdate<OrderPipelineJob>("order-pipeline", j => j.Start(null!), Cron.Hourly);
RecurringJob.AddOrUpdate<ScheduledCleanupJob>("scheduled-cleanup", j => j.Plan(null!), Cron.Daily);
RecurringJob.AddOrUpdate<CancellableExportJob>("cancellable-export", j => j.Execute(null!, null!), Cron.Daily);

app.Run();
