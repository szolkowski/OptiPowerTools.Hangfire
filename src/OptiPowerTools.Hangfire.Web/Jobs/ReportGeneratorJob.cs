using Hangfire.Console;
using Hangfire.Server;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Web.Jobs;

[WaitForOtherJobs(typeof(DataImportJob))]
public class ReportGeneratorJob
{
    public void Execute(PerformContext context)
    {
        context.WriteLine("Generating report...");
        Thread.Sleep(3_000);
        context.WriteLine("Report complete.");
    }
}
