using OptiPowerTools.Hangfire.Web;

var webProjectDir = Directory.GetCurrentDirectory();

Host.CreateDefaultBuilder(args)
    .ConfigureCmsDefaults()
    .ConfigureWebHostDefaults(webBuilder =>
    {
        webBuilder.UseStartup<Startup>();
        webBuilder.UseContentRoot(Path.GetFullPath("../../sub/MyOptiAlloySite/MyOptiAlloySite"));

        // Override MyOptiAlloySite's configuration with the web project's appsettings files,
        // then re-add environment variables so Docker env vars take precedence
        webBuilder.ConfigureAppConfiguration((context, config) =>
        {
            var env = context.HostingEnvironment;
            config.AddJsonFile(Path.Combine(webProjectDir, "appsettings.json"), optional: true, reloadOnChange: true);
            config.AddJsonFile(Path.Combine(webProjectDir, $"appsettings.{env.EnvironmentName}.json"), optional: true, reloadOnChange: true);
            config.AddEnvironmentVariables();
        });
    })
    .Build()
    .Run();
