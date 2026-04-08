using Hangfire;
using Hangfire.Console;
using Hangfire.Server;

namespace OptiPowerTools.Hangfire.Web.Samples;

/// <summary>
/// Sample job that demonstrates delayed/scheduled job execution.
/// Enqueues follow-up jobs with varying delays using <see cref="IBackgroundJobClient.Schedule"/>.
/// </summary>
public class ScheduledCleanupJob
{
    private readonly IBackgroundJobClient _jobClient;

    public ScheduledCleanupJob(IBackgroundJobClient jobClient) => _jobClient = jobClient;

    public void Plan(PerformContext context)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine("=== Scheduled Cleanup Planner ===");
        context.ResetTextColor();
        context.WriteLine("Planning staged cleanup tasks with delayed execution...");
        context.WriteLine();

        // Schedule immediate cleanup
        var immediateId = _jobClient.Enqueue<ScheduledCleanupJob>(j => j.Cleanup("temp-files", null!));
        context.WriteLine($"  Enqueued immediately: temp-files cleanup (Job {immediateId})");

        // Schedule cleanups with increasing delays
        var delay1 = TimeSpan.FromMinutes(1);
        var job1 = _jobClient.Schedule<ScheduledCleanupJob>(j => j.Cleanup("expired-sessions", null!), delay1);
        context.WriteLine($"  Scheduled in {delay1.TotalMinutes}m: expired-sessions cleanup (Job {job1})");

        var delay2 = TimeSpan.FromMinutes(5);
        var job2 = _jobClient.Schedule<ScheduledCleanupJob>(j => j.Cleanup("orphaned-media", null!), delay2);
        context.WriteLine($"  Scheduled in {delay2.TotalMinutes}m: orphaned-media cleanup (Job {job2})");

        var delay3 = TimeSpan.FromMinutes(15);
        var job3 = _jobClient.Schedule<ScheduledCleanupJob>(j => j.Cleanup("audit-logs", null!), delay3);
        context.WriteLine($"  Scheduled in {delay3.TotalMinutes}m: audit-logs cleanup (Job {job3})");

        context.WriteLine();
        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Planned 4 cleanup tasks. Check the Scheduled tab in the dashboard to see pending jobs.");
        context.ResetTextColor();
    }

    public void Cleanup(string target, PerformContext context)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine($"=== Cleaning up: {target} ===");
        context.ResetTextColor();

        var random = new Random(target.GetHashCode());
        var itemCount = random.Next(10, 100);

        for (var i = 0; i < 5; i++)
        {
            Thread.Sleep(500);
            var batch = Math.Min(itemCount - (i * itemCount / 5), itemCount / 5);
            context.WriteLine($"  Removed {batch} items from {target}...");
        }

        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Cleanup of {target} complete. Removed {itemCount} items total.");
        context.ResetTextColor();
    }
}
