using CefSharp;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace FrontEnd.Ui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewModel">ViewModel to define our behavior</param>
    /// <param name="logger">Where to send logs</param>
    public MainWindow(MainViewModel viewModel, ILogger<MainWindow> logger)
    {
        _logger = logger;

        InitializeComponent();
        DataContext = viewModel;

        // Attach to browser console messages
        // e.g. any `console.log()` calls will send output here
        Browser.ConsoleMessage += Browser_ConsoleMessage;
    }

    /// <summary>
    /// Proecess browser console messages
    /// </summary>
    /// <remarks>
    /// Simply redirects them to the system logger.
    /// May be worth considering sending them to the server for logging
    /// </remarks>
    private void Browser_ConsoleMessage(object? _, ConsoleMessageEventArgs e)
    {
        _logger.Log(
            e.Level switch
            {
                CefSharp.LogSeverity.Error => LogLevel.Error,
                CefSharp.LogSeverity.Warning => LogLevel.Warning,
                CefSharp.LogSeverity.Verbose => LogLevel.Debug,
                CefSharp.LogSeverity.Fatal => LogLevel.Critical,
                _ => LogLevel.Information
            },
            "Browser: {message}, source:{source} ({line})",
            e.Message,
            e.Source,
            e.Line
        );
    }
}