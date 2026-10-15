using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using NSubstitute;

namespace OptiPowerTools.Hangfire.Tools.Tests.Filters;

/// <summary>
/// Wires a substituted <see cref="JobStorage"/> so that a real <see cref="BackgroundJobClient"/>
/// can change a job's state, letting tests inspect the state applied by a filter's reschedule logic.
/// </summary>
internal static class RescheduleTestHelper
{
    public static IWriteOnlyTransaction SetUpStateChange(JobStorage storage, string jobId)
    {
        var connection = Substitute.For<IStorageConnection>();
        var transaction = Substitute.For<IWriteOnlyTransaction>();

        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(Substitute.For<IDisposable>());
        connection.GetJobData(jobId).Returns(new JobData
        {
            Job = Job.FromExpression(() => Console.WriteLine("")),
            State = ProcessingState.StateName,
            CreatedAt = DateTime.UtcNow
        });
        connection.CreateWriteTransaction().Returns(transaction);
        storage.GetConnection().Returns(connection);

        return transaction;
    }

    public static ScheduledState GetAppliedScheduledState(IWriteOnlyTransaction transaction, string jobId)
    {
        var call = transaction.ReceivedCalls()
            .Single(c => c.GetMethodInfo().Name == nameof(IWriteOnlyTransaction.SetJobState));
        var arguments = call.GetArguments();

        Assert.Equal(jobId, arguments[0]);
        return Assert.IsType<ScheduledState>(arguments[1]);
    }
}
