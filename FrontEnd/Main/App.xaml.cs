using System.Windows;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace FrontEnd.Main;
/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    private readonly IHost _host;

    public App()
    {
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

