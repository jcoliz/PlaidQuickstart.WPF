using CefSharp;
using Core.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace FrontEnd.Ui;

/// <summary>
/// Alternative implementation of Main Window
/// </summary>
/// <remarks>
/// In this implementation, Link is displayed in a separate window
/// </remarks>

public partial class MainWindowAlt : Window
{
    private readonly IServiceProvider _serviceProvider;
    private readonly MainViewModel _viewModel;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewModel">ViewModel to define our behavior</param>
    /// <param name="logger">Where to send logs</param>
    public MainWindowAlt(MainViewModel viewModel, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _viewModel = viewModel;

        InitializeComponent();
        DataContext = viewModel;

        // Maybe should do this in viewmodel constructor?
        _ = viewModel.UpdateLoggedInState();

        // Launch link as its own window
        _viewModel.LinkFlowStarting += LaunchLinkWindow;
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
