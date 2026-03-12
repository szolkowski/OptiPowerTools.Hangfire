using Hangfire.Console;
using Hangfire.Server;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Web.Jobs;

[ExpireOnSuccess(60)]
public class NotificationJob
{
    public void Execute(PerformContext context)
    {
        context.WriteLine("Sending notifications...");
        Thread.Sleep(2_000);
        context.WriteLine("Notifications sent.");
    }
}
