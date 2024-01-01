using System.ComponentModel;
using System.Windows;
using CefSharp;
using Core.Providers;
using Microsoft.Extensions.Logging;

namespace FrontEnd.Ui;
/// <summary>
/// Interaction logic for LinkWindow.xaml
/// </summary>
public partial class LinkWindow : Window
{
    private readonly MainViewModel _viewModel;

    public LinkWindow(MainViewModel viewModel, ILinkClient linkClient)
    {
        _viewModel = viewModel;

        InitializeComponent();
        DataContext = _viewModel;

        // Note that we cannot use XAML EventTriggers for these events, because they are triggered
        // on CEF threads
        // See https://stackoverflow.com/questions/76414363/cefsharp-with-wpf-mvvm

        // Close when link flow complete
        _viewModel.LinkFlowFinished += ViewModel_LinkFlowFinished;

        // Attach to browser console messages
        // e.g. any `console.log()` calls will send output here
        Browser.ConsoleMessage += (_,e) => _viewModel.LogBrowserConsoleMessage(e);

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

    private void ViewModel_LinkFlowFinished(object? sender, EventArgs e)
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
        // Need to remove this because the viewmodel will continue to live on,
        // after this class is disposed.
        _viewModel.LinkFlowFinished -= ViewModel_LinkFlowFinished;
        base.OnClosing(e);
    }
}
