using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using CefSharp;
using Microsoft.Extensions.Logging;

namespace FrontEnd.Ui;
/// <summary>
/// Interaction logic for LinkWindow.xaml
/// </summary>
public partial class LinkWindow : Window
{
    private readonly ILogger<MainWindow> _logger;

    public LinkWindow(MainViewModel viewModel, ILogger<MainWindow> logger)
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
