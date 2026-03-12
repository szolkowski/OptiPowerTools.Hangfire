using Hangfire.Console;
using Hangfire.Server;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Web.Jobs;

[RetainOnSuccess(180)]
public class MonthlyAuditJob
{
    public void Execute(PerformContext context)
    {
        context.WriteLine("Starting monthly audit...");
        Thread.Sleep(3_000);
        context.WriteLine("Monthly audit complete.");
    }
}
