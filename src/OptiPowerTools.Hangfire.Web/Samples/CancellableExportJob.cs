using Hangfire;
using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;

namespace OptiPowerTools.Hangfire.Web.Samples;

/// <summary>
/// Sample job that demonstrates cancellation token support for long-running operations.
/// Delete the job from the dashboard while it's running to see graceful cancellation in action.
/// </summary>
public class CancellableExportJob
{
    private const int TotalRecords = 500;

    public void Execute(PerformContext context, IJobCancellationToken cancellationToken)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine("=== Full Data Export (Cancellable) ===");
        context.ResetTextColor();
        context.WriteLine($"Exporting {TotalRecords} records. Delete this job from the dashboard to cancel.");
        context.WriteLine();

        var progressBar = context.WriteProgressBar("Export");
        var exported = 0;

        try
        {
            for (var i = 0; i < TotalRecords; i++)
            {
                cancellationToken.ThrowIfCancellationRequested();

                Thread.Sleep(50);
                exported++;

                if (exported % 50 == 0)
                {
                    context.WriteLine($"  Exported {exported}/{TotalRecords} records...");
                }

                progressBar.SetValue(exported * 100.0 / TotalRecords);
            }

            context.WriteLine();
            context.SetTextColor(ConsoleTextColor.Green);
            context.WriteLine($"Export complete. {exported} records written.");
            context.ResetTextColor();
        }
        catch (OperationCanceledException)
        {
            context.WriteLine();
            context.SetTextColor(ConsoleTextColor.Yellow);
            context.WriteLine($"Export cancelled after {exported}/{TotalRecords} records.");
            context.WriteLine("Partial export file cleaned up.");
            context.ResetTextColor();
            throw;
        }
    }
}
