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
using CefSharp.Wpf;
using Core.Providers;
using Microsoft.Extensions.Logging;

namespace FrontEnd.Ui;
/// <summary>
/// Interaction logic for LinkWindow.xaml
/// </summary>
public partial class LinkWindow : Window
{
    private readonly MainViewModel _viewModel;
    private readonly ILogger<LinkWindow> _logger;

    public LinkWindow(MainViewModel viewModel, ILinkClient linkClient, ILogger<LinkWindow> logger)
    {
        _viewModel = viewModel;
        _logger = logger;

        InitializeComponent();
        DataContext = _viewModel;

        // Attach to browser console messages
        // e.g. any `console.log()` calls will send output here
        Browser.ConsoleMessage += Browser_ConsoleMessage;

        // Attach to posted messages
        Browser.JavascriptMessageReceived += Browser_JavascriptMessageReceived;

        // Register link client for JS
        Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        Browser.JavascriptObjectRepository.Register(
            "linkClient",
            linkClient,       
            options: BindingOptions.DefaultBinder
        );
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
        // TODO: Consider whether it would be better to push this into the view model

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

        if (e.Level == LogSeverity.Error)
        {
            _viewModel.LastErrorMessage = e.Message;        
        }
    }

    /// <summary>
    /// Receive message from browser
    /// </summary>
    /// <remarks>
    /// All messages mean we should close the window
    /// </remarks>
    /// <param name="sender">Browser which sent it</param>
    /// <param name="e">Event details</param>
    private void Browser_JavascriptMessageReceived(object? _, JavascriptMessageReceivedEventArgs e)
    {
        // TODO: Consider whether it would be better to push this into the view model

        var success = (bool)e.Message;
        _logger.LogInformation("Browser: Received message {message}", success);

        // This event is called on a CEF Thread.
        // We have some UI work now, will send through dispatcher
        Dispatcher.BeginInvoke(() =>
        {
            // Clear the error, if successful
            if (success)
            {
                _viewModel.LastErrorMessage = null;            
            }

            // Update the logged in state
            _ = _viewModel.UpdateLoggedInState();

            // Close this window
            Close();
        });
    }
}
