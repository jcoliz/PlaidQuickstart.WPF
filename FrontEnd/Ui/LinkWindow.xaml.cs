using System;
using System.Collections.Generic;
using System.ComponentModel;
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
        Browser.ConsoleMessage += (_,e) => _viewModel.LogBrowserConsoleMessage(e);

        // Attach to posted messages
        Browser.JavascriptMessageReceived += (_,e) => _viewModel.ReceiveJavascriptMessage(e);

        // Close when link flow complete
        _viewModel.LinkFlowFinished += viewModel_LinkFlowFinished;

        // Register link client for JS
        Browser.JavascriptObjectRepository.Settings.LegacyBindingEnabled = true;
        Browser.JavascriptObjectRepository.Register(
            "linkClient",
            linkClient,       
            options: BindingOptions.DefaultBinder
        );
    }

    private void viewModel_LinkFlowFinished(object? sender, EventArgs e)
    {
        // This event is called on a CEF Thread.
        // We have some UI work now, will send through dispatcher
        Dispatcher.BeginInvoke(() =>
        {
            // Close this window
            Close();
        });
    }

    protected override void OnClosing(CancelEventArgs e)
    {
        _viewModel.LinkFlowFinished -= viewModel_LinkFlowFinished;
        base.OnClosing(e);
    }
}
