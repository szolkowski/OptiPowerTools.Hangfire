using OptiPowerTools.Hangfire.Web;

var webProjectDir = Directory.GetCurrentDirectory();

Host.CreateDefaultBuilder(args)
    .ConfigureCmsDefaults()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseContentRoot(Path.GetFullPath("../../sub/foundation/src/Foundation"));

        // Override Foundation's configuration with the web project's appsettings files
        webBuilder.ConfigureAppConfiguration((context, config) =>
        {
            var env = context.HostingEnvironment;
            config.AddJsonFile(Path.Combine(webProjectDir, "appsettings.json"), optional: true, reloadOnChange: true);
            config.AddJsonFile(Path.Combine(webProjectDir, $"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: true);
        });
    })
    .Build()
    .Run();
