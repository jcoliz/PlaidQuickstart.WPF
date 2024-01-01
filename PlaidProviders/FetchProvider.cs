// Copyright (C) 2024 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using Core.Models;
using Core.Providers;
using Going.Plaid;
using Going.Plaid.Accounts;
using Going.Plaid.Entity;
using Going.Plaid.Institutions;
using Going.Plaid.Transactions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Data;
using System.Net;
using System.Runtime.CompilerServices;

namespace PlaidProviders;

/// <summary>
/// Fetch data directly from Plaid Services
/// </summary>
/// <remarks>
/// Would not be used in production on client
/// </remarks>
public class FetchProvider(ILogger<FetchProvider> _logger, IOptions<PlaidCredentials> _credentials, PlaidClient _client) : IFetchClient
{
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

        var result = new WireDataTable
        {
            Columns = ColumnsFrom("Name", "Amount/r", "Date/r", "Category", "Channel"),
            Rows = added
                .OrderBy(x => x.Date)
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

    public async Task<WireDataTable> Institutions()
    {
        CheckCredentials();

        // Note that this endpoint does NOT want an access token set on the client
        _client.AccessToken = null;

        // Accumulated rows received sofar
        var rows = new List<Row>();

        // How many items to get per call
        const int chunk_size = 500;

        // Total number of items already fetched
        var fetched_count = 0;

        // Number of items which need to get fetched
        var fetch_needed = int.MaxValue;

        while (fetched_count < fetch_needed)
        {
            var request = new InstitutionsGetRequest()
            {
                Count = chunk_size,
                Offset = fetched_count,
                CountryCodes = _credentials.Value!.CountryCodes!.Split(',').Select(p => Enum.Parse<CountryCode>(p, true)).ToArray(),
            };

            var response = await _client.InstitutionsGetAsync(request);

            if (response.Error is not null)
            {
                throw Error(response.StatusCode,response.Error);
            }

            rows.AddRange(
                response.Institutions
                .Select(x =>
                    new Row(
                        x.Name,
                        x.InstitutionId,
                        x.Status?.ToString() ?? "null"
                    )
                )
            );

            fetched_count += response.Institutions.Count;
            fetch_needed = response.Total;
        }

        var result = new WireDataTable
        {
            Columns = ColumnsFrom("Name", "Id", "Status"),
            Rows = [.. rows]
        };

        _logger.LogInformation("Institutions: OK {num} rows", result.Rows.Length);

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

    private PlaidServiceException Error(HttpStatusCode statusCode, PlaidError error, [CallerMemberName] string callerName = "")
    {
        var result = new PlaidServiceException((int)statusCode, error);
        _logger.LogError(result, "{caller}: FAILED {status}", callerName, statusCode);

        return result;
    }

    private void CheckCredentials()
    {
        if (_credentials == null || _credentials.Value == null)
        {
            throw new ArgumentNullException(nameof(_credentials), "Please supply Plaid credentials in .NET configuration");
        }
    }

    private void CheckCredentialsLoggedIn()
    {
        CheckCredentials();

        if (_credentials.Value!.AccessToken == null)
        {
            throw new ArgumentNullException(nameof(_credentials.Value.AccessToken), "Must be logged in to complete this operation");
        }

        // Set to latest access token, in case it has changed
        _client.AccessToken = _credentials.Value.AccessToken;
    }
}

