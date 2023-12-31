using System.ComponentModel;
using System.Data;
using System.Windows.Input;
using System.Windows.Threading;
using CefSharp;
using Core.Models;
using Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontEnd.Ui;

/// <summary>
/// View Model for the main interaction window
/// </summary>
/// <param name="fetchClient">Client we will use to fetch bank data</param>
public class MainViewModel(
    ILogger<MainViewModel> logger, 
    IOptions<UiSettings> settings, 
    IOptions<AppSettings> appSettings, 
    IFetchClient fetchClient,
    ILinkClient linkClient
)
    : INotifyPropertyChanged
{
    /// <summary>
    /// Web location of the home page
    /// </summary>
    public Uri? WebAddress
    {
        get
        {
            var web_address = settings.Value?.WebAddress;

            if (web_address is null)
            {
                logger.LogError("Must set WebAddress in UI settings");
                return null;
            }
            else
            {
                var result = new Uri(web_address);
                logger.LogInformation("Using web address {uri}", result);
                return result;
            }
        }
    }

    /// <summary>
    /// Fires when something has changed
    /// </summary>
    public event PropertyChangedEventHandler? PropertyChanged;

    /// <summary>
    /// Fires when Link is starting
    /// </summary>
    public event EventHandler? LinkFlowStarting;

    /// <summary>
    /// Fires when Link is complete
    /// </summary>
    public event EventHandler? LinkFlowFinished;

    /// <summary>
    /// Initiate fetching of balances
    /// </summary>
    public ICommand FetchBalancesCommand => _FetchBalancesCommand ??= new CommandHandler(_ => FetchBalances(), () => IsLoggedIn);
    private ICommand? _FetchBalancesCommand;

    /// <summary>
    /// Initiate fetching of transactions
    /// </summary>
    public ICommand FetchTransactionsCommand => _FetchTransactionsCommand ??= new CommandHandler(_ => FetchTransactions(), () => IsLoggedIn);
    private ICommand? _FetchTransactionsCommand;

    /// <summary>
    /// Initiate fetching of transactions
    /// </summary>
    public ICommand FetchInstitutionsCommand => _FetchInstitutionsCommand ??= new CommandHandler(_ => FetchInstitutions(), () => true);
    private ICommand? _FetchInstitutionsCommand;

    /// <summary>
    /// Initiate logging out
    /// </summary>
    public ICommand LogOutCommand => _LogOutCommand ??= new CommandHandler(_ => DoLogOut(), () => true);
    private ICommand? _LogOutCommand;

    /// <summary>
    /// Initiate logging out
    /// </summary>
    public ICommand StartLinkCommand => _StartLinkCommand ??= new CommandHandler(_ => LinkFlowStarting?.Invoke(this, new EventArgs()), () => true);
    private ICommand? _StartLinkCommand;

    /// <summary>
    /// Whether we currently KNOW if we're logged in or not
    /// </summary>
    /// <remarks>
    /// The knowledge of whether we're logged in or not may live remotely.
    /// It would be a network-bound call to find out. This property will hold
    /// 'true' if we have received a definitiive result from the Link Provider  
    /// that we are or are not logged in.
    /// </remarks>
    public bool IsLoggedInStatusKnown
    {
        get => _IsLoggedInStatusKnown;
        private set
        {
            if (_IsLoggedInStatusKnown != value)
            {
                _IsLoggedInStatusKnown = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
        }
    }
    private bool _IsLoggedInStatusKnown = false;

    /// <summary>
    /// Whether user is currently logged in
    /// </summary>
    public bool IsLoggedIn
    {
        get => _IsLoggedIn;
        private set
        {
            if (_IsLoggedIn != value)
            {
                _IsLoggedIn = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsLoggedIn)));
            }
        }
    }
    private bool _IsLoggedIn = false;

    /// <summary>
    /// Most recently reported error
    /// </summary>
    public string? LastErrorMessage
    {
        get => _LastErrorMessage;
        set
        {
            if (_LastErrorMessage != value)
            {
                _LastErrorMessage = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(LastErrorMessage)));
            }
        }
    }
    private string? _LastErrorMessage;

    /// <summary>
    /// Latest balances data from server
    /// </summary>
    public DataView? BalancesData
    {
        get; private set;
    }

    /// <summary>
    /// Latest transactions data, fetched from server
    /// </summary>
    public DataView? TransactionsData
    {
        get; private set;
    }

    /// <summary>
    /// Latest result from institutions fetch
    /// </summary>
    public string? InstitutionsResult
    {
        get => _InstitutionsResult;
        set
        {
            if (_InstitutionsResult != value)
            {
                _InstitutionsResult = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(InstitutionsResult)));
            }
        }
    }
    private string _InstitutionsResult = "Operation not attempted";

    /// <summary>
    /// Display name of application
    /// </summary>
    public string AppName => appSettings.Value?.Name ?? nameof(MainViewModel);

    public async Task UpdateLoggedInState()
    {
        IsLoggedIn = await linkClient.IsLoggedIn();
        IsLoggedInStatusKnown = true;
    }

    /// <summary>
    /// Do the work of feteching balances
    /// </summary>
    protected async void FetchBalances()
    {
        try
        {
            var data = await fetchClient!.Balance();
            var table = data.ToClientDataTable();
            BalancesData = table.AsDataView();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FetchBalances: FAILED");
            BalancesData = null;
        }
        finally
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BalancesData)));
        }
    }

    /// <summary>
    /// Do the work of fetching transactions
    /// </summary>
    protected async void FetchTransactions()
    {
        try
        {
            var data = await fetchClient!.Transactions();
            var table = data.ToClientDataTable();
            TransactionsData = table.AsDataView();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FetchTransactions: FAILED");
            TransactionsData = null;
        }
        finally
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TransactionsData)));
        }
    }

    /// <summary>
    /// Do the work of fetching institutions
    /// </summary>
    protected async void FetchInstitutions()
    {
        try
        {
            InstitutionsResult = $"Fetching...";
            var data = await fetchClient!.Institutions();
            var num_rows = data.Rows.Length;
            InstitutionsResult = $"Fetch OK. {num_rows} rows fetched.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FetchInstitutions: FAILED");
            InstitutionsResult = ex.ToString();
        }
    }

    /// <summary>
    /// Do the work of logging out
    /// </summary>
    protected async void DoLogOut()
    {
        await linkClient.LogOut();
        await UpdateLoggedInState();    
    }

    /// <summary>
    /// Process browser console messages
    /// </summary>
    /// <remarks>
    /// Simply redirects them to the system logger.
    /// May be worth considering sending them to the server for logging
    /// TODO: Make into an ICommand
    /// </remarks>
    public void LogBrowserConsoleMessage(ConsoleMessageEventArgs e)
    {
        logger.Log(
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
            LastErrorMessage = e.Message;
        }
    }

    /// <summary>
    /// Receive message from browser
    /// </summary>
    /// <remarks>
    /// All messages mean we should close the window
    /// </remarks>
    /// <param name="e">Event details</param>
    public void ReceiveJavascriptMessage( JavascriptMessageReceivedEventArgs e)
    {
        var success = (bool)e.Message;
     
        logger.LogInformation("Browser: Received message {message}", success);

        if (success)
        {
            LastErrorMessage = null;
        }

        _ = UpdateLoggedInState();

        LinkFlowFinished?.Invoke(this, new EventArgs());
    }
}

internal static class Extensions
{
    /// <summary>
    /// Convert the data table we got over the network (wire) into the form that the
    /// UI can display
    /// </summary>
    /// <param name="t">Source table</param>
    /// <returns>Resulting table</returns>
    public static DataTable ToClientDataTable(this WireDataTable t)
    {
        var table = new DataTable();
        table.Columns.AddRange(t.Columns.Select(x => new DataColumn(x.Title)).ToArray());
        foreach (var row in t.Rows)
        {
            table.Rows.Add(row.Cells);
        }
        return table;
    }
}
