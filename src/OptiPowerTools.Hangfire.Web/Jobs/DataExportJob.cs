using Hangfire.Console;
using Hangfire.Server;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Web.Jobs;

[MutualExclusion("data-pipeline")]
public class DataExportJob
{
    public void Execute(PerformContext context)
    {
        context.WriteLine("Starting data export...");
        Thread.Sleep(5_000);
        context.WriteLine("Data export complete.");
    }
}
