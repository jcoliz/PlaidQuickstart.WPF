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

    }

    private async void LinkButton_Click(object sender, RoutedEventArgs e)
    {
        var linkwindow = new LinkWindow((DataContext as MainViewModel)!, _logger);
        linkwindow!.Owner = this;
        linkwindow.ShowDialog();
    }
}