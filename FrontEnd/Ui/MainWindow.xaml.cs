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
    private readonly ILogger<MainWindow> _logger;
    private readonly IServiceProvider _serviceProvider;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewModel">ViewModel to define our behavior</param>
    /// <param name="logger">Where to send logs</param>
    public MainWindow(MainViewModel viewModel, ILogger<MainWindow> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;

        InitializeComponent();
        DataContext = viewModel;

    }

    private void LinkButton_Click(object sender, RoutedEventArgs e)
    {
        var linkwindow = _serviceProvider.GetRequiredService<LinkWindow>();
        linkwindow!.Owner = this;
        linkwindow.ShowDialog();
    }
}