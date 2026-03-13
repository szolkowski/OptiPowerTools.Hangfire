using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.States;

namespace OptiPowerTools.Hangfire.Tools.Filters;

/// <summary>
/// Prevents a job from executing while any of the specified job types are currently processing.
/// This is a one-directional check — only the decorated job needs the attribute.
/// When a conflict is detected, the job is rescheduled after <see cref="RetryDelaySeconds"/>
/// and the worker thread is freed immediately.
/// </summary>
/// <remarks>
/// Uses the Hangfire monitoring API to check for processing jobs. There is a small race-condition
/// window between the check and execution. For guaranteed mutual exclusion, use
/// <see cref="MutualExclusionAttribute"/> instead.
/// For same-type exclusion, combine with Hangfire's built-in <c>[DisableConcurrentExecution]</c>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class WaitForOtherJobsAttribute : JobFilterAttribute, IServerFilter
{
    private readonly Type[] _jobTypes;

    /// <summary>
    /// Gets or sets the delay in seconds before the job is re-enqueued after a conflict.
    /// </summary>
    public int RetryDelaySeconds { get; init; } = 15;

    /// <summary>
    /// Gets or sets the maximum number of processing jobs to check for conflicts.
    /// </summary>
    public int MaxJobsToCheck { get; init; } = 1000;

    /// <summary>
    /// Initializes a new instance of <see cref="WaitForOtherJobsAttribute"/>.
    /// </summary>
    /// <param name="jobTypes">
    /// The job types to check. If any of these types are currently processing,
    /// the decorated job will be rescheduled.
    /// </param>
    public WaitForOtherJobsAttribute(params Type[] jobTypes)
    {
        _jobTypes = jobTypes ?? throw new ArgumentNullException(nameof(jobTypes));
    }

    /// <inheritdoc />
    public void OnPerforming(PerformingContext context)
    {
        if (_jobTypes.Length == 0)
            return;

        var monitoringApi = context.Storage.GetMonitoringApi();

        var targetTypeNames = new HashSet<string>(
            _jobTypes
                .Where(t => t.FullName is not null)
                .Select(t => t.FullName!));

        var currentJobId = context.BackgroundJob.Id;

        const int pageSize = 50;
        for (var offset = 0; offset < MaxJobsToCheck; offset += pageSize)
        {
            var count = Math.Min(pageSize, MaxJobsToCheck - offset);
            var page = monitoringApi.ProcessingJobs(offset, count);
            if (page.Count == 0)
                break;

            var hasConflict = page.Any(j =>
                j.Key != currentJobId
                && j.Value?.Job?.Type is not null
                && j.Value.Job.Type.FullName is { } fullName
                && targetTypeNames.Contains(fullName));

            if (hasConflict)
            {
                RescheduleJob(context);
                return;
            }
        }
    }

    /// <inheritdoc />
    public void OnPerformed(PerformedContext context)
    {
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
                Reason = $"Waiting for other jobs to complete: {string.Join(", ", _jobTypes.Select(t => t.Name))}"
            });
    }
}
