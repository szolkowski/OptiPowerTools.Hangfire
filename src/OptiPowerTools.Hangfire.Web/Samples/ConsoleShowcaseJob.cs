using Hangfire.Console;
using Hangfire.Console.Progress;
using Hangfire.Server;

namespace OptiPowerTools.Hangfire.Web.Samples;

/// <summary>
/// Sample job that demonstrates Hangfire.Console features by processing fake product data.
/// </summary>
public class ConsoleShowcaseJob
{
    private static readonly string[] _categories = ["Electronics", "Clothing", "Home & Garden", "Sports", "Books"];

    private static readonly string[] _products =
    [
        "Wireless Headphones", "Running Shoes", "Coffee Maker", "Yoga Mat", "Sci-Fi Novel",
        "Bluetooth Speaker", "Winter Jacket", "Desk Lamp", "Tennis Racket", "Cookbook",
        "Laptop Stand", "Hiking Boots", "Air Purifier", "Dumbbells", "Travel Guide",
        "USB-C Hub", "Rain Coat", "Toaster Oven", "Soccer Ball", "Mystery Novel",
        "Webcam", "Sneakers", "Vacuum Cleaner", "Resistance Bands", "Biography",
    ];

    public void Execute(PerformContext context)
    {
        var random = new Random(42);

        context.WriteLine("=== Product Catalog Sync ===");
        context.WriteLine($"Started at {DateTime.UtcNow:u}");
        context.WriteLine();

        // --- Phase 1: Validate categories ---
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine("Phase 1: Validating categories...");
        context.ResetTextColor();

        foreach (var category in _categories)
        {
            Thread.Sleep(200);
            context.SetTextColor(ConsoleTextColor.Green);
            context.WriteLine($"  [OK] {category}");
            context.ResetTextColor();
        }

        context.WriteLine($"Validated {_categories.Length} categories.");
        context.WriteLine();

        // --- Phase 2: Process products with progress bar ---
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine("Phase 2: Processing products...");
        context.ResetTextColor();

        var progressBar = context.WriteProgressBar("Products");

        var processed = 0;
        var skipped = 0;
        var errors = 0;

        for (var i = 0; i < _products.Length; i++)
        {
            var product = _products[i];
            var category = _categories[random.Next(_categories.Length)];
            var price = Math.Round(random.NextDouble() * 200 + 5, 2);

            Thread.Sleep(300);

            // Simulate occasional skips and errors
            if (i == 7)
            {
                context.SetTextColor(ConsoleTextColor.Yellow);
                context.WriteLine($"  [SKIP] {product} — duplicate detected, skipping");
                context.ResetTextColor();
                skipped++;
            }
            else if (i == 18)
            {
                context.SetTextColor(ConsoleTextColor.Red);
                context.WriteLine($"  [ERR]  {product} — invalid price data, queued for review");
                context.ResetTextColor();
                errors++;
            }
            else
            {
                context.WriteLine($"  [SYNC] {product} | {category} | ${price}");
                processed++;
            }

            progressBar.SetValue((i + 1) * 100.0 / _products.Length);
        }

        context.WriteLine();

        // --- Phase 3: Summary ---
        context.SetTextColor(ConsoleTextColor.Cyan);
        context.WriteLine("Phase 3: Summary");
        context.ResetTextColor();

        context.WriteLine($"  Processed: {processed}");

        context.SetTextColor(ConsoleTextColor.Yellow);
        context.WriteLine($"  Skipped:   {skipped}");
        context.ResetTextColor();

        context.SetTextColor(ConsoleTextColor.Red);
        context.WriteLine($"  Errors:    {errors}");
        context.ResetTextColor();

        context.WriteLine();
        context.SetTextColor(ConsoleTextColor.Green);
        context.WriteLine($"Catalog sync completed at {DateTime.UtcNow:u}");
        context.ResetTextColor();
    }
}
