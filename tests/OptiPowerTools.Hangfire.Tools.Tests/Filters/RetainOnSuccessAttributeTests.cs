using Hangfire;
using Hangfire.Common;
using Hangfire.States;
using Hangfire.Storage;
using NSubstitute;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Tools.Tests.Filters;

public class RetainOnSuccessAttributeTests
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
    public void Constructor_DefaultValue_SetsRetentionDaysTo90()
    {
        var filter = new RetainOnSuccessAttribute();

        Assert.Equal(90, filter.RetentionDays);
    }

    [Fact]
    public void Constructor_WithCustomValue_SetsRetentionDays()
    {
        var filter = new RetainOnSuccessAttribute(180);

        Assert.Equal(180, filter.RetentionDays);
    }

    [Fact]
    public void Constructor_WithZeroValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetainOnSuccessAttribute(0));
    }

    [Fact]
    public void Constructor_WithNegativeValue_ThrowsArgumentOutOfRangeException()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new RetainOnSuccessAttribute(-1));
    }

    [Fact]
    public void RetentionDays_CanBeSetViaProperty()
    {
        var filter = new RetainOnSuccessAttribute { RetentionDays = 365 };

        Assert.Equal(365, filter.RetentionDays);
    }

    [Fact]
    public void OnStateApplied_SucceededState_SetsJobExpirationTimeout()
    {
        var context = CreateApplyStateContext(newState: new SucceededState(0, 0, 0));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new RetainOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(TimeSpan.FromDays(90), context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_SucceededState_CustomRetention_SetsCorrectTimeout()
    {
        var context = CreateApplyStateContext(newState: new SucceededState(0, 0, 0));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new RetainOnSuccessAttribute(180);

        filter.OnStateApplied(context, transaction);

        Assert.Equal(TimeSpan.FromDays(180), context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_FailedState_DoesNotChangeExpiration()
    {
        var context = CreateApplyStateContext(newState: new FailedState(new Exception("test")));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var originalTimeout = context.JobExpirationTimeout;
        var filter = new RetainOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(originalTimeout, context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_ScheduledState_DoesNotChangeExpiration()
    {
        var context = CreateApplyStateContext(newState: new ScheduledState(TimeSpan.FromMinutes(5)));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var originalTimeout = context.JobExpirationTimeout;
        var filter = new RetainOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(originalTimeout, context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateApplied_DeletedState_DoesNotChangeExpiration()
    {
        var context = CreateApplyStateContext(newState: new DeletedState());
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var originalTimeout = context.JobExpirationTimeout;
        var filter = new RetainOnSuccessAttribute();

        filter.OnStateApplied(context, transaction);

        Assert.Equal(originalTimeout, context.JobExpirationTimeout);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(30)]
    [InlineData(90)]
    [InlineData(365)]
    public void OnStateApplied_SucceededState_SetsCorrectTimeSpan(int days)
    {
        var context = CreateApplyStateContext(newState: new SucceededState(0, 0, 0));
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new RetainOnSuccessAttribute(days);

        filter.OnStateApplied(context, transaction);

        Assert.Equal(TimeSpan.FromDays(days), context.JobExpirationTimeout);
    }

    [Fact]
    public void OnStateUnapplied_DoesNotThrow()
    {
        var context = CreateApplyStateContext();
        var transaction = Substitute.For<IWriteOnlyTransaction>();
        var filter = new RetainOnSuccessAttribute();

        var exception = Record.Exception(() => filter.OnStateUnapplied(context, transaction));

        Assert.Null(exception);
    }
}
