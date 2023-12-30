using CefSharp;
using Core.Providers;
using Microsoft.Extensions.Logging;
using System.Windows;

namespace FrontEnd.Ui;

/// <summary>
/// Interaction logic for MainWindow.xaml
/// </summary>
public partial class MainWindow : Window
{
    private readonly ILogger<MainWindow> _logger;
    private readonly ILinkClient _linkClient;

    /// <summary>
    /// Constructor
    /// </summary>
    /// <param name="viewModel">ViewModel to define our behavior</param>
    /// <param name="logger">Where to send logs</param>
    public MainWindow(MainViewModel viewModel, ILinkClient linkClient, ILogger<MainWindow> logger)
    {
        _logger = logger;
        _linkClient = linkClient;

        InitializeComponent();
        DataContext = viewModel;

    }

    private void LinkButton_Click(object sender, RoutedEventArgs e)
    {
        var linkwindow = new LinkWindow((DataContext as MainViewModel)!, _linkClient, _logger);
        linkwindow!.Owner = this;
        linkwindow.ShowDialog();
    }
}