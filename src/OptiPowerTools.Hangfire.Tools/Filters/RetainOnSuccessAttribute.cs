using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;

namespace OptiPowerTools.Hangfire.Tools.Filters;

/// <summary>
/// Extends the retention period for succeeded jobs beyond Hangfire's default
/// of 24 hours. Use this for infrequent jobs (weekly reports, monthly audits)
/// where you want execution details to remain visible in the dashboard until
/// the next run.
/// </summary>
/// <remarks>
/// This filter only affects succeeded jobs; failed or deleted jobs retain
/// their default Hangfire expiration. For the inverse (reducing retention),
/// see <see cref="ExpireOnSuccessAttribute"/>.
/// </remarks>
[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class RetainOnSuccessAttribute : JobFilterAttribute, IApplyStateFilter
{
    /// <summary>
    /// Gets or sets the retention period in days after a job succeeds.
    /// </summary>
    public int RetentionDays { get; init; }

    /// <summary>
    /// Initializes a new instance of <see cref="RetainOnSuccessAttribute"/>.
    /// </summary>
    /// <param name="retentionDays">
    /// Days to retain succeeded job data. Defaults to 90. Must be positive.
    /// </param>
    public RetainOnSuccessAttribute(int retentionDays = 90)
    {
        if (retentionDays <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(retentionDays),
                "Value must be positive.");
        }

        RetentionDays = retentionDays;
    }

    /// <inheritdoc />
    public void OnStateApplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
        if (context.NewState is SucceededState)
        {
            context.JobExpirationTimeout = TimeSpan.FromDays(RetentionDays);
        }
    }

    /// <inheritdoc />
    public void OnStateUnapplied(ApplyStateContext context, IWriteOnlyTransaction transaction)
    {
    }
}
