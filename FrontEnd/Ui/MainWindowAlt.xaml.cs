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

    public MainWindowAlt(MainViewModel viewModel, IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        _viewModel = viewModel;

        InitializeComponent();
        DataContext = viewModel;

        // Launch link as its own window, when we start showing Link
        _viewModel.PropertyChanged += (s, e) =>
        {
            if (e.PropertyName == nameof(MainViewModel.IsShowingLink) && _viewModel.IsShowingLink)
            {
                // Need to run this through the dispatcher, otherwise we will be blocking here,
                // which we definitely do not want!!
                Dispatcher.BeginInvoke(() => LaunchLinkWindow());
            }
        };
    }

    /// <summary>
    /// Launch Link as its own window
    /// </summary>
    private void LaunchLinkWindow()
    {
        // Open a separate window to display link flow in

        var linkwindow = _serviceProvider.GetRequiredService<LinkWindow>();
        linkwindow!.Owner = this;
        linkwindow.ShowDialog();
    }
}
