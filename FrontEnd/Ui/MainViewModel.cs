using System.ComponentModel;
using System.Data;
using System.Windows.Input;
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
    /// Initiate fetching of balances
    /// </summary>
    public ICommand FetchBalancesCommand => _FetchBalancesCommand ??= new CommandHandler(() => FetchBalances(), () => true);
    private ICommand? _FetchBalancesCommand;

    /// <summary>
    /// Initiate fetching of transactions
    /// </summary>
    public ICommand FetchTransactionsCommand => _FetchTransactionsCommand ??= new CommandHandler(() => FetchTransactions(), () => true);
    private ICommand? _FetchTransactionsCommand;

    /// <summary>
    /// Initiate logging out
    /// </summary>
    public ICommand LogOutCommand => _LogOutCommand ??= new CommandHandler(() => DoLogOut(), () => true);
    private ICommand? _LogOutCommand;

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
    /// Do the work of logging out
    /// </summary>
    protected async void DoLogOut()
    {
        await linkClient.LogOut();
        await UpdateLoggedInState();    
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
