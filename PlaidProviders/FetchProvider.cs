using System.Data;
using System.Net;
using System.Runtime.CompilerServices;
using Going.Plaid;
using Going.Plaid.Accounts;
using Going.Plaid.Entity;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Core.Models;
using Core.Providers;

namespace PlaidProviders;

/// <summary>
/// Fetch data directly from Plaid Services
/// </summary>
/// <remarks>
/// Would not be used in production on client
/// </remarks>
public class FetchProvider: IFetchClient
{
    private readonly ILogger<FetchProvider> _logger;
    private readonly PlaidCredentials _credentials;
    private readonly PlaidClient _client;

    public FetchProvider(ILogger<FetchProvider> logger, IOptions<PlaidCredentials> credentials, PlaidClient client)
    {
        _logger = logger;
        _credentials = credentials.Value;
        _client = client;
    }

    public async Task<WireDataTable> Balance()
    {
        CheckCredentialsLoggedIn();

        var request = new AccountsBalanceGetRequest();

        var response = await _client.AccountsBalanceGetAsync(request);

        if (response.Error is not null)
        {
            throw Error(response.Error);
        }

        var result = new WireDataTable
        {
            Columns = ColumnsFrom("Name", "AccountId", "Balance/r"),
            Rows = response.Accounts
                .Select(x =>
                    new Row(
                        x.Name,
                        x.AccountId,
                        x.Balances?.Current?.ToString("C2") ?? string.Empty
                    )
                )
                .ToArray()
        };

        _logger.LogInformation("Balance: OK {num} rows", result.Rows.Length);

        return result;
    }

    public async Task<WireDataTable> Transactions()
    {
        CheckCredentialsLoggedIn();

        // Set cursor to empty to receive all historical updates
        var cursor = string.Empty;

        // New transaction updates since "cursor"
        var added = new List<Transaction>();
        var modified = new List<Transaction>();
        var removed = new List<RemovedTransaction>();
        var hasMore = true;

        while (hasMore)
        {
            const int numrequested = 100;
            var request = new TransactionsSyncRequest()
            {
                Cursor = cursor,
                Count = numrequested
            };

            var response = await _client.TransactionsSyncAsync(request);

            if (response.Error is not null)
            {
                throw Error(response.Error);
            }

            added.AddRange(response!.Added);
            modified.AddRange(response.Modified);
            removed.AddRange(response.Removed);
            hasMore = response.HasMore;
            cursor = response.NextCursor;
        }

        const int numresults = 8;
        var result = new WireDataTable
        {
            Columns = ColumnsFrom("Name", "Amount/r", "Date/r", "Category", "Channel"),
            Rows = added
                .OrderBy(x => x.Date)
                .TakeLast(numresults)
                .Select(x =>
                    new Row(
                        x.Name ?? string.Empty,
                        x.Amount?.ToString("C2") ?? string.Empty,
                        x.Date?.ToShortDateString() ?? string.Empty,
                        string.Join(':', x.Category ?? Enumerable.Empty<string>()),
                        x.PaymentChannel.ToString() ?? string.Empty
                    )
                )
                .ToArray()
        };

        _logger.LogInformation("Transactions: OK {num} rows", result.Rows.Length);

        return result;
    }

    private static Column[] ColumnsFrom(params string[] cols) =>
        cols.Select(x =>
            {
                var split = x.Split("/");
                return new Column() { Title = split[0], IsRight = split.Length > 1 && split[1] == "r" };
            }).ToArray();

    private PlaidServiceException Error(PlaidError error, [CallerMemberName] string callerName = "")
    {
        var result = new PlaidServiceException((int)HttpStatusCode.BadRequest, error);
        _logger.LogError(result, "{caller}: FAILED", callerName);

        return result;
    }

    private void CheckCredentialsLoggedIn()
    {
        if (_credentials == null)
        {
            throw new ArgumentNullException(nameof(_credentials), "Please supply Plaid credentials in .NET configuration");
        }
        if (_credentials.AccessToken == null)
        {
            throw new ArgumentNullException(nameof(_credentials.AccessToken), "Must be logged in to complete this operation");
        }

        // Set to latest access token, in case it has changed
        _client.AccessToken = _credentials.AccessToken;
    }
}

