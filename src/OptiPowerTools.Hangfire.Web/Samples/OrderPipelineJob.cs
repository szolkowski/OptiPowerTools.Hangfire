using Hangfire;
using Hangfire.Console;
using Hangfire.Server;

namespace OptiPowerTools.Hangfire.Web.Samples;

/// <summary>
/// Sample job that demonstrates job continuations by chaining a multi-step order pipeline.
/// Each step is a separate job linked via <see cref="IBackgroundJobClient.ContinueJobWith"/>.
/// </summary>
public class OrderPipelineJob
{
    private readonly IBackgroundJobClient _jobClient;

    public OrderPipelineJob(IBackgroundJobClient jobClient) => _jobClient = jobClient;

    public void Start(PerformContext context)
    {
        var orderId = $"ORD-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}";

        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine($"=== Order Pipeline Started: {orderId} ===");
        context.ResetTextColor();
        context.WriteLine("Scheduling continuation chain: Validate -> Payment -> Ship -> Notify");
        context.WriteLine();

        var validateId = _jobClient.Enqueue<OrderPipelineJob>(j => j.Validate(orderId, null!));
        var paymentId = _jobClient.ContinueJobWith<OrderPipelineJob>(validateId, j => j.ProcessPayment(orderId, null!));
        var shipId = _jobClient.ContinueJobWith<OrderPipelineJob>(paymentId, j => j.Ship(orderId, null!));
        _jobClient.ContinueJobWith<OrderPipelineJob>(shipId, j => j.NotifyCustomer(orderId, null!));

        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Continuation chain created. First job: {validateId}");
        context.ResetTextColor();
    }

    public void Validate(string orderId, PerformContext context)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine($"[Step 1/4] Validating order {orderId}...");
        context.ResetTextColor();

        Thread.Sleep(1_500);
        context.WriteLine("  Checking inventory... OK");
        Thread.Sleep(500);
        context.WriteLine("  Verifying shipping address... OK");
        Thread.Sleep(500);
        context.WriteLine("  Applying discount codes... OK");

        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Order {orderId} validated.");
        context.ResetTextColor();
    }

    public void ProcessPayment(string orderId, PerformContext context)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine($"[Step 2/4] Processing payment for {orderId}...");
        context.ResetTextColor();

        var amount = Math.Round(Random.Shared.NextDouble() * 500 + 20, 2);

        Thread.Sleep(2_000);
        context.WriteLine($"  Charging ${amount} to card ending in ****4242...");
        Thread.Sleep(1_000);
        context.WriteLine($"  Transaction ID: TXN-{Random.Shared.Next(100000, 999999)}");

        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Payment for {orderId} completed.");
        context.ResetTextColor();
    }

    public void Ship(string orderId, PerformContext context)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine($"[Step 3/4] Shipping order {orderId}...");
        context.ResetTextColor();

        Thread.Sleep(1_000);
        var trackingNumber = $"TRACK-{Random.Shared.Next(100000000, 999999999)}";
        context.WriteLine($"  Generating shipping label...");
        Thread.Sleep(1_000);
        context.WriteLine($"  Tracking number: {trackingNumber}");
        Thread.Sleep(500);
        context.WriteLine($"  Estimated delivery: {DateTime.UtcNow.AddDays(3):yyyy-MM-dd}");

        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Order {orderId} shipped.");
        context.ResetTextColor();
    }

    public void NotifyCustomer(string orderId, PerformContext context)
    {
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine($"[Step 4/4] Notifying customer about {orderId}...");
        context.ResetTextColor();

        Thread.Sleep(1_000);
        context.WriteLine("  Sending order confirmation email...");
        Thread.Sleep(500);
        context.WriteLine("  Sending shipping notification...");
        Thread.Sleep(500);

        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"=== Order Pipeline Complete: {orderId} ===");
        context.ResetTextColor();
    }
}
