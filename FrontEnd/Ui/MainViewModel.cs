using CefSharp;
using Core.Models;
using Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.ComponentModel;
using System.Data;
using System.Windows.Input;

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
    : INotifyPropertyChanged, IPageStatus
{
    #region Events
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

    #endregion

    #region Commands

    /// <summary>
    /// Initiate fetching of balances
    /// </summary>
    public ICommand FetchBalancesCommand => _FetchBalancesCommand ??= new CommandHandler(_ => FetchBalances(), () => IsLoggedIn && !IsFetchingBalances);
    private ICommand? _FetchBalancesCommand;

    /// <summary>
    /// Initiate fetching of transactions
    /// </summary>
    public ICommand FetchTransactionsCommand => _FetchTransactionsCommand ??= new CommandHandler(_ => FetchTransactions(), () => IsLoggedIn && !IsFetchingTransactions);
    private ICommand? _FetchTransactionsCommand;

    /// <summary>
    /// Initiate fetching of transactions
    /// </summary>
    public ICommand FetchInstitutionsCommand => _FetchInstitutionsCommand ??= new CommandHandler(_ => FetchInstitutions(), () => !IsFetchingInstitutions);
    private ICommand? _FetchInstitutionsCommand;

    /// <summary>
    /// Initiate logging out
    /// </summary>
    public ICommand LogOutCommand => _LogOutCommand ??= new CommandHandler(_ => DoLogOut(), () => IsLoggedIn);
    private ICommand? _LogOutCommand;

    /// <summary>
    /// Initiate logging out
    /// </summary>
    public ICommand StartLinkCommand => _StartLinkCommand ??= new CommandHandler(_ => LaunchLink(), () => true);
    private ICommand? _StartLinkCommand;

    #endregion

    #region Properties

    /// <summary>
    /// Current web location to display
    /// </summary>
    public Uri? WebAddress
    {
        get => _WebAddress ?? _BlankWebAddress;
        private set
        {
            if (_WebAddress != value)
            {
                _WebAddress = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(WebAddress)));
            }
        }
    }
    private Uri? _WebAddress;
    private readonly Uri _BlankWebAddress = new("about:blank");

    public bool IsShowingLink
    {
        get => _IsShowingLink;
        private set
        {
            if (_IsShowingLink != value)
            {
                _IsShowingLink = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsShowingLink)));
            }
        }
    }
    private bool _IsShowingLink = false;

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
    private string? _InstitutionsResult = "Operation not attempted";

    /// <summary>
    /// Display name of application
    /// </summary>
    public string AppName => appSettings.Value?.Name ?? nameof(MainViewModel);

    #endregion

    #region Methods

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
    /// Update whether we are logged in or not
    /// </summary>
    /// <returns>
    /// TODO: THis should be called from constructor, and then made private
    /// </returns>

    public async Task UpdateLoggedInState()
    {
        IsLoggedIn = await linkClient.IsLoggedIn();
        IsLoggedInStatusKnown = true;
    }

    #endregion

    #region IPageStatus

    // NOTE: Would prefer for these to be direct implementations of the interface,
    // e.g. "IPageStatus.LinkLoading", however CEF object registration doesn't
    // deal with interfaces correctly.

    /// <summary>
    /// Report that Link is loading now
    /// </summary>
    public void LinkLoading()
    {
        logger.LogInformation("Page Status: Loading");

        // Here we could display an indication to the user
    }
    /// <summary>
    /// Report that Link is running now
    /// </summary>
    public void LinkRunning()
    {
        logger.LogInformation("Page Status: Running");

        // Here we could display an indication to the user
    }
    /// <summary>
    /// Report that Link has completed successfully now
    /// </summary>
    public void LinkSuccess()
    {
        logger.LogInformation("Page Status: Success");

        LastErrorMessage = null;

        _ = UpdateLoggedInState();

        WebAddress = null;
        LinkFlowFinished?.Invoke(this, new EventArgs());
        IsShowingLink = false;
    }
    /// <summary>
    /// Report that Link has failed now
    /// </summary>
    public void LinkFailed(string reason)
    {
        logger.LogError("Page Status: Failed {reason}", reason);

        LastErrorMessage = reason;

        _ = UpdateLoggedInState();

        WebAddress = null;
        LinkFlowFinished?.Invoke(this, new EventArgs());
        IsShowingLink = false;
    }
    #endregion

    #region Internal Operations

    /// <summary>
    /// Web location of the home page
    /// </summary>
    protected Uri ConfiguredWebAddress
    {
        get
        {
            var web_address = settings.Value?.WebAddress;

            if (web_address is null)
            {
                logger.LogError("Must set WebAddress in UI settings");
                return _BlankWebAddress;
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
    /// Do the work of feteching balances
    /// </summary>
    protected async void FetchBalances()
    {
        try
        {
            IsFetchingBalances = true;
            BalancesData?.Table?.Clear();
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
            IsFetchingBalances = false;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(BalancesData)));
        }
    }

    private bool IsFetchingBalances = false;
    private bool IsFetchingTransactions = false;
    private bool IsFetchingInstitutions = false;

    /// <summary>
    /// Do the work of fetching transactions
    /// </summary>
    protected async void FetchTransactions()
    {
        try
        {
            IsFetchingTransactions = true;
            TransactionsData?.Table?.Clear();
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
            IsFetchingTransactions = false;
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
            IsFetchingInstitutions = true;
            InstitutionsResult = "Fetching...";
            var data = await fetchClient!.Institutions();
            var num_rows = data.Rows.Length;
            InstitutionsResult = $"Fetch OK. {num_rows} rows fetched.";
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "FetchInstitutions: FAILED");
            InstitutionsResult = ex.ToString();
        }
        IsFetchingInstitutions = false;
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
    /// Begin the Link flow
    /// </summary>
    protected void LaunchLink()
    {
        LinkLoading();
        LinkFlowStarting?.Invoke(this, new EventArgs());
        IsShowingLink = true;
        WebAddress = ConfiguredWebAddress;
    }

    #endregion
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
