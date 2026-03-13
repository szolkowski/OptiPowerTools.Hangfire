using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace OptiPowerTools.Hangfire.Tools.Filters;

/// <summary>
/// Reduces the retention period for succeeded jobs. By default, Hangfire keeps
/// succeeded jobs for 24 hours. This filter overrides the expiration timeout
/// when a job transitions to <see cref="SucceededState"/>, allowing short-lived
/// jobs to be cleaned up faster and reducing dashboard clutter.
/// </summary>
/// <remarks>
/// A value of 0 (the default) means the job is expired with minimal retention.
/// This filter only affects succeeded jobs; failed or deleted jobs retain
/// their default Hangfire expiration.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class ExpireOnSuccessAttribute : JobFilterAttribute, IApplyStateFilter
{
    /// <summary>
    /// Gets or sets the expiration timeout in seconds after a job succeeds.
    /// </summary>
    public int ExpirationSeconds { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="ExpireOnSuccessAttribute"/>.
    /// </summary>
    /// <param name="expirationSeconds">
    /// Seconds until the succeeded job data expires. Defaults to 0 (minimal retention).
    /// </param>
    public ExpireOnSuccessAttribute(int expirationSeconds = 0)
    {
        if (expirationSeconds < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expirationSeconds),
                "Value must be non-negative.");
        }

        ExpirationSeconds = expirationSeconds;
    }

    /// <inheritdoc />
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is SucceededState)
        {
            context.JobExpirationTimeout = TimeSpan.FromSeconds(ExpirationSeconds);
        }
    }

    /// <inheritdoc />
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}
