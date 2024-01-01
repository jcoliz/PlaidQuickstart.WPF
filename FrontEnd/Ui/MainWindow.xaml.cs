using CefSharp;
using Core.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace FrontEnd.Ui;

/// <summary>
/// As-designed implementation of Main Window
/// </summary>
/// <remarks>
/// In this implementation, Link is displayed within the main window
/// </remarks>
public partial class MainWindow : Window
{
    private readonly MainViewModel _viewModel;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewModel">ViewModel to define our behavior</param>
    /// <param name="logger">Where to send logs</param>
    public MainWindow(MainViewModel viewModel, ILinkClient linkClient)
    {
        _viewModel = viewModel;

        InitializeComponent();
        DataContext = viewModel;

        // Attach to browser console messages
        // e.g. any `console.log()` calls will send output here
        Browser.ConsoleMessage += (_, e) => _viewModel.LogBrowserConsoleMessage(e);

        // Register link client for JS
        Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        Browser.JavascriptObjectRepository.Register(
            "linkClient",
            linkClient,
            options: BindingOptions.DefaultBinder
        );

        // Register page status reporting for JS
        IPageStatus pageStatus = _viewModel;
        Browser.JavascriptObjectRepository.Register(
            "pageStatus",
            pageStatus,
            options: BindingOptions.DefaultBinder
        );
    }
}
