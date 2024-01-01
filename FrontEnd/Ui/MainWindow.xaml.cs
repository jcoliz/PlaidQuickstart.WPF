using CefSharp;
using Core.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace FrontEnd.Ui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _viewModel;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewModel">ViewModel to define our behavior</param>
    /// <param name="logger">Where to send logs</param>
    public MainWindow(MainViewModel viewModel, ILinkClient linkClient, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _viewModel = viewModel;

        InitializeComponent();
        DataContext = viewModel;

        // Maybe should do this in viewmodel constructor?
        _ = viewModel.UpdateLoggedInState();

#if SEPARATE_LINK_WINDOW
        // Launch link as its own window
        _viewModel.LinkFlowStarting += LaunchLinkWindow;
#else
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
#endif
    }

    /// <summary>
    /// Launch Link as its own window
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    private void LaunchLinkWindow(object? sender, EventArgs e)
    {
        // Open a separate window to display link flow in

        var linkwindow = _serviceProvider.GetRequiredService<LinkWindow>();
        linkwindow!.Owner = this;
        linkwindow.ShowDialog();
    }
}
