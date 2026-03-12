using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using NSubstitute;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Tools.Tests.Filters;

public class ExpireOnSuccessAttributeTests
{
    private static ApplyStateContext CreateApplyStateContext(
        IState? newState = null,
        string? oldStateName = null)
    {
        var storage = Substitute.For<JobStorage>();
        var connection = Substitute.For<IStorageConnection>();
        var transaction = Substitute.For<IWriteOnlyTransaction>();

        var job = Job.FromExpression(() => Console.WriteLine(""));
        var backgroundJob = new BackgroundJob("test-job-id", job, DateTime.UtcNow);

        newState ??= new SucceededState(0, 0, 0);
        oldStateName ??= "Processing";

        return new ApplyStateContext(
            storage,
            connection,
            transaction,
            backgroundJob,
            newState,
            oldStateName);
    }

    [Fact]
    public void Constructor_DefaultValue_SetsExpirationSecondsToZero()
    {
        var filter = new ExpireOnSuccessAttribute();

        Assert.Equal(0, filter.ExpirationSeconds);
    }

    [Fact]
    public void Constructor_WithCustomValue_SetsExpirationSeconds()
    {
        var filter = new ExpireOnSuccessAttribute(300);

        Assert.Equal(300, filter.ExpirationSeconds);
    }

    [Fact]
    public void Constructor_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ExpireOnSuccessAttribute(-1));
    }

    [Fact]
    public void ExpirationSeconds_CanBeSetViaProperty()
    {
        var filter = new ExpireOnSuccessAttribute { ExpirationSeconds = 120 };

        Assert.Equal(120, filter.ExpirationSeconds);
    }

    [Fact]
    public void OnStateApplied_SucceededState_SetsJobExpirationTimeout()
    {
        var context = CreateApplyStateContext(newState: new SucceededState(0, 0, 0));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new ExpireOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(TimeSpan.Zero, context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_SucceededState_CustomExpiration_SetsCorrectTimeout()
    {
        var context = CreateApplyStateContext(newState: new SucceededState(0, 0, 0));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new ExpireOnSuccessAttribute(300);

        filter.OnStateApplied(context, transaction);

        Assert.Equal(TimeSpan.FromSeconds(300), context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_FailedState_DoesNotChangeExpiration()
    {
        var context = CreateApplyStateContext(newState: new FailedState(new Exception("test")));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var originalTimeout = context.JobExpirationTimeout;
        var filter = new ExpireOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(originalTimeout, context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_ScheduledState_DoesNotChangeExpiration()
    {
        var context = CreateApplyStateContext(newState: new ScheduledState(TimeSpan.FromMinutes(5)));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var originalTimeout = context.JobExpirationTimeout;
        var filter = new ExpireOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(originalTimeout, context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_DeletedState_DoesNotChangeExpiration()
    {
        var context = CreateApplyStateContext(newState: new DeletedState());
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var originalTimeout = context.JobExpirationTimeout;
        var filter = new ExpireOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(originalTimeout, context.JobExpirationTimeout);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(60)]
    [InlineData(3600)]
    public void OnStateApplied_SucceededState_SetsCorrectTimeSpan(int seconds)
    {
        var context = CreateApplyStateContext(newState: new SucceededState(0, 0, 0));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new ExpireOnSuccessAttribute(seconds);

        filter.OnStateApplied(context, transaction);

        Assert.Equal(TimeSpan.FromSeconds(seconds), context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateUnapplied_DoesNotThrow()
    {
        var context = CreateApplyStateContext();
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new ExpireOnSuccessAttribute();

        var exception = Record.Exception(() => filter.OnStateUnapplied(context, transaction));

        Assert.Null(exception);
    }
}
