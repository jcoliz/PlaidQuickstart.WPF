using FrontEnd.Ui;
using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace FrontEnd.Main;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
        //
        // Set up a console window
        //
        // Useful to observe logs
        // Remove this if you don't want to see a console window with logs
        //
        ConsoleAllocator.ShowConsoleWindow();

        //
        // Set up .NET generic host
        //
        // https://learn.microsoft.com/en-us/dotnet/core/extensions/generic-host
        //
        _host = new HostBuilder()
            .ConfigureAppConfiguration((context, configurationBuilder) =>
            {
                // Config files will be found in the content root path
                configurationBuilder.SetBasePath(context.HostingEnvironment.ContentRootPath);

                // Add an application-specific config file
                configurationBuilder.AddJsonFile("appsettings.json", optional: false);

                // Enable picking up configuration from the environment vars
                configurationBuilder.AddEnvironmentVariables();
            })
            .ConfigureServices((context, services) =>
            {                
                services.AddSingleton<MainWindow>();
                services.Configure<UiSettings>(context.Configuration.GetSection(UiSettings.Section));
            })
            .ConfigureLogging((context, logging) =>
            {
                // Get log configuration out of `Logging` section in configuration
                logging.AddConfiguration(context.Configuration.GetSection("Logging"));

                // Send logs to the console window
                // (Only useful if console window has been created)
                logging.AddConsole();

                // Send logs to debug console
                // (Only useful if running in Visual Studio)
                logging.AddDebug();
            })
            .Build();
    }

    private async void Application_Startup(object sender, StartupEventArgs e)
    {
        await _host.StartAsync();

        var mainWindow = _host.Services.GetService<MainWindow>();
        mainWindow!.Show();
    }

    private async void Application_Exit(object sender, ExitEventArgs e)
    {
        using (_host)
        {
            await _host.StopAsync(TimeSpan.FromSeconds(5));
        }
    }
}

