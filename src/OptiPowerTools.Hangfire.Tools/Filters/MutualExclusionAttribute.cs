using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;
using Hangfire.Storage;

namespace OptiPowerTools.Hangfire.Tools.Filters;

/// <summary>
/// Prevents concurrent execution of jobs sharing the same resource group name.
/// All jobs decorated with the same resource name are mutually exclusive.
/// When a conflict is detected, the job is rescheduled after <see cref="RetryDelaySeconds"/>
/// and the worker thread is freed immediately.
/// </summary>
/// <remarks>
/// This filter uses Hangfire distributed locks for reliable mutual exclusion with no race conditions.
/// For one-directional type-based exclusion, use <see cref="WaitForOtherJobsAttribute"/> instead.
/// For same-type exclusion, combine with Hangfire's built-in <c>[DisableConcurrentExecution]</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class MutualExclusionAttribute : JobFilterAttribute, IServerFilter
{
    private const string LockItemKey = "MutualExclusion:Lock";
    private readonly string _resourceName;

    /// <summary>
    /// Gets or sets the delay in seconds before the job is re-enqueued after a conflict.
    /// </summary>
    public int RetryDelaySeconds { get; set; } = 15;

    /// <summary>
    /// Initializes a new instance of <see cref="MutualExclusionAttribute"/>.
    /// </summary>
    /// <param name="resourceName">
    /// A shared resource group name. All jobs decorated with the same name cannot run concurrently.
    /// </param>
    public MutualExclusionAttribute(string resourceName)
    {
        _resourceName = resourceName ?? throw new ArgumentNullException(nameof(resourceName));
    }

    /// <inheritdoc />
    public void OnPerforming(PerformingContext context)
    {
        try
        {
            var distributedLock = context.Connection.AcquireDistributedLock(
                $"hangfire:mutual-exclusion:{_resourceName}",
                TimeSpan.Zero);

            context.Items[LockItemKey] = distributedLock;
        }
        catch (DistributedLockTimeoutException)
        {
            RescheduleJob(context);
        }
    }

    /// <inheritdoc />
    public void OnPerformed(PerformedContext context)
    {
        if (context.Items.TryGetValue(LockItemKey, out var lockObj)
            && lockObj is IDisposable distributedLock)
        {
            distributedLock.Dispose();
            context.Items.Remove(LockItemKey);
        }
    }

    /// <summary>
    /// Cancels job execution and reschedules it for later.
    /// </summary>
    protected virtual void RescheduleJob(PerformingContext context)
    {
        context.Canceled = true;

        var client = new BackgroundJobClient(context.Storage);
        client.ChangeState(
            context.BackgroundJob.Id,
            new ScheduledState(TimeSpan.FromSeconds(RetryDelaySeconds))
            {
                Reason = $"Resource '{_resourceName}' is locked by another job"
            });
    }
}
