using Hangfire.Console;
using Hangfire.Server;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Web.Jobs;

[MutualExclusion("data-pipeline")]
public class DataImportJob
{
    public void Execute(PerformContext context)
    {
        context.WriteLine("Starting data import...");
        Thread.Sleep(10_000);
        context.WriteLine("Data import complete.");
    }
}
