using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using Hangfire.Storage.Monitoring;
using NSubstitute;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Tools.Tests.Filters;

public class WaitForOtherJobsAttributeTests
{
    private class StubJobA
    {
        public void Execute() { }
    }

    private class StubJobB
    {
        public void Execute() { }
    }

    private class StubJobC
    {
        public void Execute() { }
    }

    private class TestableWaitForOtherJobs : WaitForOtherJobsAttribute
    {
        public bool RescheduleJobCalled { get; private set; }

        public TestableWaitForOtherJobs(params Type[] jobTypes) : base(jobTypes) { }

        protected override void RescheduleJob(PerformingContext context)
        {
            RescheduleJobCalled = true;
            context.Canceled = true;
        }
    }

    private static PerformingContext CreatePerformingContext(
        JobStorage? storage = null,
        string jobId = "current-job-id")
    {
        var connection = Substitute.For<IStorageConnection>();
        storage ??= Substitute.For<JobStorage>();

        var job = Job.FromExpression(() => Console.WriteLine(""));
        var backgroundJob = new BackgroundJob(jobId, job, DateTime.UtcNow);
        var cancellationToken = Substitute.For<IJobCancellationToken>();

        return new PerformingContext(
            new PerformContext(storage, connection, backgroundJob, cancellationToken));
    }

    private static PerformedContext CreatePerformedContext()
    {
        var connection = Substitute.For<IStorageConnection>();
        var storage = Substitute.For<JobStorage>();

        var job = Job.FromExpression(() => Console.WriteLine(""));
        var backgroundJob = new BackgroundJob("test-job-id", job, DateTime.UtcNow);
        var cancellationToken = Substitute.For<IJobCancellationToken>();

        return new PerformedContext(
            new PerformContext(storage, connection, backgroundJob, cancellationToken),
            null,
            false,
            null);
    }

    private static JobStorage CreateStorageWithProcessingJobs(
        params KeyValuePair<string, ProcessingJobDto>[] processingJobs)
    {
        var storage = Substitute.For<JobStorage>();
        var monitoringApi = Substitute.For<IMonitoringApi>();
        monitoringApi.ProcessingJobs(Arg.Any<int>(), Arg.Any<int>())
            .Returns(new JobList<ProcessingJobDto>(processingJobs));
        storage.GetMonitoringApi().Returns(monitoringApi);
        return storage;
    }

    private static KeyValuePair<string, ProcessingJobDto> CreateProcessingJob(
        string jobId, Type jobType)
    {
        var job = new Job(jobType, jobType.GetMethod("Execute")!);
        var dto = new ProcessingJobDto { Job = job };
        return new KeyValuePair<string, ProcessingJobDto>(jobId, dto);
    }

    [Fact]
    public void Constructor_WithJobTypes_DoesNotThrow()
    {
        var filter = new WaitForOtherJobsAttribute(typeof(StubJobA), typeof(StubJobB));

        Assert.NotNull(filter);
    }

    [Fact]
    public void Constructor_WithEmptyTypes_DoesNotThrow()
    {
        var filter = new WaitForOtherJobsAttribute();

        Assert.NotNull(filter);
    }

    [Fact]
    public void Constructor_WithNullTypes_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new WaitForOtherJobsAttribute(null!));
    }

    [Fact]
    public void RetryDelaySeconds_DefaultIs15()
    {
        var filter = new WaitForOtherJobsAttribute(typeof(StubJobA));

        Assert.Equal(15, filter.RetryDelaySeconds);
    }

    [Fact]
    public void RetryDelaySeconds_CanBeSet()
    {
        var filter = new WaitForOtherJobsAttribute(typeof(StubJobA)) { RetryDelaySeconds = 30 };

        Assert.Equal(30, filter.RetryDelaySeconds);
    }

    [Fact]
    public void OnPerforming_NoProcessingJobs_DoesNotReschedule()
    {
        var storage = CreateStorageWithProcessingJobs();
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA));

        filter.OnPerforming(context);

        Assert.False(filter.RescheduleJobCalled);
        Assert.False(context.Canceled);
    }

    [Fact]
    public void OnPerforming_EmptyJobTypes_DoesNotReschedule()
    {
        var storage = CreateStorageWithProcessingJobs(
            CreateProcessingJob("other-job", typeof(StubJobA)));
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs();

        filter.OnPerforming(context);

        Assert.False(filter.RescheduleJobCalled);
        Assert.False(context.Canceled);
    }

    [Fact]
    public void OnPerforming_ProcessingJobMatchesType_ReschedulesJob()
    {
        var storage = CreateStorageWithProcessingJobs(
            CreateProcessingJob("other-job", typeof(StubJobA)));
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA));

        filter.OnPerforming(context);

        Assert.True(filter.RescheduleJobCalled);
        Assert.True(context.Canceled);
    }

    [Fact]
    public void OnPerforming_ProcessingJobDoesNotMatchType_DoesNotReschedule()
    {
        var storage = CreateStorageWithProcessingJobs(
            CreateProcessingJob("other-job", typeof(StubJobB)));
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA));

        filter.OnPerforming(context);

        Assert.False(filter.RescheduleJobCalled);
        Assert.False(context.Canceled);
    }

    [Fact]
    public void OnPerforming_SelfJobInProcessing_DoesNotReschedule()
    {
        var storage = CreateStorageWithProcessingJobs(
            CreateProcessingJob("current-job-id", typeof(StubJobA)));
        var context = CreatePerformingContext(storage: storage, jobId: "current-job-id");
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA));

        filter.OnPerforming(context);

        Assert.False(filter.RescheduleJobCalled);
        Assert.False(context.Canceled);
    }

    [Fact]
    public void OnPerforming_MultipleTypesOneMatch_ReschedulesJob()
    {
        var storage = CreateStorageWithProcessingJobs(
            CreateProcessingJob("other-job", typeof(StubJobB)));
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA), typeof(StubJobB));

        filter.OnPerforming(context);

        Assert.True(filter.RescheduleJobCalled);
        Assert.True(context.Canceled);
    }

    [Fact]
    public void OnPerforming_NullJobInProcessingList_DoesNotThrow()
    {
        var nullDto = new KeyValuePair<string, ProcessingJobDto>(
            "null-job", new ProcessingJobDto { Job = null });
        var storage = CreateStorageWithProcessingJobs(nullDto);
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA));

        var exception = Record.Exception(() => filter.OnPerforming(context));

        Assert.Null(exception);
        Assert.False(filter.RescheduleJobCalled);
    }

    [Fact]
    public void OnPerforming_MultipleProcessingJobsMixedTypes_ReschedulesOnMatch()
    {
        var storage = CreateStorageWithProcessingJobs(
            CreateProcessingJob("job-1", typeof(StubJobC)),
            CreateProcessingJob("job-2", typeof(StubJobA)));
        var context = CreatePerformingContext(storage: storage);
        var filter = new TestableWaitForOtherJobs(typeof(StubJobA));

        filter.OnPerforming(context);

        Assert.True(filter.RescheduleJobCalled);
    }

    [Fact]
    public void OnPerformed_DoesNotThrow()
    {
        var context = CreatePerformedContext();
        var filter = new WaitForOtherJobsAttribute(typeof(StubJobA));

        var exception = Record.Exception(() => filter.OnPerformed(context));

        Assert.Null(exception);
    }
}
