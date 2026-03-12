using Hangfire;
using Hangfire.Common;
using Hangfire.Server;
using Hangfire.Storage;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using OptiPowerTools.Hangfire.Tools.Filters;

namespace OptiPowerTools.Hangfire.Tools.Tests.Filters;

public class MutualExclusionAttributeTests
{
    private const string _testResource = "test-resource";

    private static PerformingContext CreatePerformingContext(
        IStorageConnection? connection = null,
        JobStorage? storage = null,
        string jobId = "test-job-id")
    {
        connection ??= Substitute.For<IStorageConnection>();
        storage ??= Substitute.For<JobStorage>();

        var job = Job.FromExpression(() => Console.WriteLine(""));
        var backgroundJob = new BackgroundJob(jobId, job, DateTime.UtcNow);
        var cancellationToken = Substitute.For<IJobCancellationToken>();

        return new PerformingContext(
            new PerformContext(storage, connection, backgroundJob, cancellationToken));
    }

    private static PerformedContext CreatePerformedContext(
        IStorageConnection? connection = null,
        JobStorage? storage = null,
        string jobId = "test-job-id")
    {
        connection ??= Substitute.For<IStorageConnection>();
        storage ??= Substitute.For<JobStorage>();

        var job = Job.FromExpression(() => Console.WriteLine(""));
        var backgroundJob = new BackgroundJob(jobId, job, DateTime.UtcNow);
        var cancellationToken = Substitute.For<IJobCancellationToken>();

        return new PerformedContext(
            new PerformContext(storage, connection, backgroundJob, cancellationToken),
            null,
            false,
            null);
    }

    private class TestableMutualExclusion(string resourceName) : MutualExclusionAttribute(resourceName)
    {
        public bool RescheduleJobCalled { get; private set; }

        protected override void RescheduleJob(PerformingContext context)
        {
            RescheduleJobCalled = true;
            context.Canceled = true;
        }
    }

    [Fact]
    public void Constructor_WithResourceName_SetsDefaults()
    {
        var filter = new MutualExclusionAttribute(_testResource);

        Assert.Equal(15, filter.RetryDelaySeconds);
    }

    [Fact]
    public void Constructor_WithNullResourceName_ThrowsArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() => new MutualExclusionAttribute(null!));
    }

    [Fact]
    public void RetryDelaySeconds_CanBeSet()
    {
        var filter = new MutualExclusionAttribute(_testResource) { RetryDelaySeconds = 30 };

        Assert.Equal(30, filter.RetryDelaySeconds);
    }

    [Fact]
    public void OnPerforming_LockAcquired_StoresLockInItems()
    {
        var connection = Substitute.For<IStorageConnection>();
        var mockLock = Substitute.For<IDisposable>();
        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(mockLock);

        var context = CreatePerformingContext(connection: connection);
        var filter = new MutualExclusionAttribute(_testResource);

        filter.OnPerforming(context);

        Assert.True(context.Items.ContainsKey("MutualExclusion:Lock"));
        Assert.Same(mockLock, context.Items["MutualExclusion:Lock"]);
        Assert.False(context.Canceled);
    }

    [Fact]
    public void OnPerforming_AcquiresLockWithCorrectResourceName()
    {
        var connection = Substitute.For<IStorageConnection>();
        var mockLock = Substitute.For<IDisposable>();
        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(mockLock);

        var context = CreatePerformingContext(connection: connection);
        var filter = new MutualExclusionAttribute("my-resource");

        filter.OnPerforming(context);

        connection.Received(1).AcquireDistributedLock(
            "hangfire:mutual-exclusion:my-resource",
            Arg.Any<TimeSpan>());
    }

    [Fact]
    public void OnPerforming_AcquiresLockWithZeroTimeout()
    {
        var connection = Substitute.For<IStorageConnection>();
        var mockLock = Substitute.For<IDisposable>();
        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(mockLock);

        var context = CreatePerformingContext(connection: connection);
        var filter = new MutualExclusionAttribute(_testResource);

        filter.OnPerforming(context);

        connection.Received(1).AcquireDistributedLock(
            Arg.Any<string>(),
            TimeSpan.Zero);
    }

    [Fact]
    public void OnPerforming_LockTimeout_ReschedulesJob()
    {
        var connection = Substitute.For<IStorageConnection>();
        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Throws(new DistributedLockTimeoutException(_testResource));

        var context = CreatePerformingContext(connection: connection);
        var filter = new TestableMutualExclusion(_testResource);

        filter.OnPerforming(context);

        Assert.True(filter.RescheduleJobCalled);
        Assert.True(context.Canceled);
    }

    [Fact]
    public void OnPerforming_LockTimeout_DoesNotStoreLockInItems()
    {
        var connection = Substitute.For<IStorageConnection>();
        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Throws(new DistributedLockTimeoutException(_testResource));

        var context = CreatePerformingContext(connection: connection);
        var filter = new TestableMutualExclusion(_testResource);

        filter.OnPerforming(context);

        Assert.False(context.Items.ContainsKey("MutualExclusion:Lock"));
    }

    [Fact]
    public void OnPerformed_WithLock_DisposesLock()
    {
        var mockLock = Substitute.For<IDisposable>();
        var context = CreatePerformedContext();
        context.Items["MutualExclusion:Lock"] = mockLock;

        var filter = new MutualExclusionAttribute(_testResource);

        filter.OnPerformed(context);

        mockLock.Received(1).Dispose();
        Assert.False(context.Items.ContainsKey("MutualExclusion:Lock"));
    }

    [Fact]
    public void OnPerformed_WithoutLock_DoesNotThrow()
    {
        var context = CreatePerformedContext();
        var filter = new MutualExclusionAttribute(_testResource);

        var exception = Record.Exception(() => filter.OnPerformed(context));

        Assert.Null(exception);
    }

    [Fact]
    public void OnPerformed_WithNonDisposableLock_DoesNotThrow()
    {
        var context = CreatePerformedContext();
        context.Items["MutualExclusion:Lock"] = "not-a-disposable";

        var filter = new MutualExclusionAttribute(_testResource);

        var exception = Record.Exception(() => filter.OnPerformed(context));

        Assert.Null(exception);
    }

    [Fact]
    public void LockTimeoutSeconds_DefaultIsZero()
    {
        var filter = new MutualExclusionAttribute(_testResource);

        Assert.Equal(0, filter.LockTimeoutSeconds);
    }

    [Fact]
    public void LockTimeoutSeconds_CanBeSet()
    {
        var filter = new MutualExclusionAttribute(_testResource) { LockTimeoutSeconds = 5 };

        Assert.Equal(5, filter.LockTimeoutSeconds);
    }

    [Fact]
    public void OnPerforming_UsesConfiguredLockTimeout()
    {
        var connection = Substitute.For<IStorageConnection>();
        var mockLock = Substitute.For<IDisposable>();
        connection.AcquireDistributedLock(Arg.Any<string>(), Arg.Any<TimeSpan>())
            .Returns(mockLock);

        var context = CreatePerformingContext(connection: connection);
        var filter = new MutualExclusionAttribute(_testResource) { LockTimeoutSeconds = 10 };

        filter.OnPerforming(context);

        connection.Received(1).AcquireDistributedLock(
            Arg.Any<string>(),
            TimeSpan.FromSeconds(10));
    }

    [Fact]
    public void OnPerformed_WhenDisposeThrows_DoesNotThrow()
    {
        var mockLock = Substitute.For<IDisposable>();
        mockLock.When(x => x.Dispose()).Do(_ => throw new InvalidOperationException("Connection lost"));
        var context = CreatePerformedContext();
        context.Items["MutualExclusion:Lock"] = mockLock;

        var filter = new MutualExclusionAttribute(_testResource);

        var exception = Record.Exception(() => filter.OnPerformed(context));

        Assert.Null(exception);
        Assert.False(context.Items.ContainsKey("MutualExclusion:Lock"));
    }
}
