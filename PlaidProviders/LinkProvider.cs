using Core.Models;
using Core.Providers;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Net;
using System.Runtime.CompilerServices;

namespace PlaidProviders;

/// <summary>
/// Connects to Plaid services directly to provide Link services
/// </summary>
/// <remarks>
/// Would not be used in production on client
/// </remarks>
/// <param name="logger">Where to log</param>
/// <param name="credentials">Credentials of logged-in user</param>
/// <param name="client">Client to use for connection</param>
public class LinkProvider(ILogger<LinkProvider> logger, IOptions<AppSettings> appSettings, IOptions<PlaidCredentials> credentials, PlaidClient client) : ILinkClient
{
    public async Task<string> CreateLinkToken()
    {
        CheckCredentials();

        var request = new LinkTokenCreateRequest()
        {
            AccessToken = credentials.Value!.AccessToken,
            User = new LinkTokenCreateRequestUser { ClientUserId = Guid.NewGuid().ToString(), },
            ClientName = appSettings?.Value?.Name ?? ".NET Link Provider",
            Products = credentials.Value!.Products!.Split(',').Select(p => Enum.Parse<Products>(p, true)).ToArray(),
            Language = Enum.Parse<Language>(credentials.Value?.Language ?? "English"),
            CountryCodes = credentials.Value!.CountryCodes!.Split(',').Select(p => Enum.Parse<CountryCode>(p, true)).ToArray(),
        };
        var response = await client.LinkTokenCreateAsync(request);

        if (response.Error is not null)
        {
            throw Error(response.Error);
        }

        logger.LogInformation("CreateLinkToken: OK {token}", response.LinkToken);

        return response.LinkToken;
    }

    public async Task<PlaidCredentials> ExchangePublicToken(LinkResult link)
    {
        CheckCredentials();

        var request = new ItemPublicTokenExchangeRequest()
        {
            PublicToken = link.public_token!
        };

        var response = await client.ItemPublicTokenExchangeAsync(request);

        if (response.Error is not null)
        {
            throw Error(response.Error);
        }

        credentials.Value!.AccessToken = response.AccessToken;
        credentials.Value!.ItemId = response.ItemId;

        logger.LogInformation("ExchangePublicToken: OK {item}", response.ItemId);

        return credentials.Value;
    }
    public Task<PlaidCredentials> Info()
    {
        CheckCredentials();

        logger.LogInformation("Info: OK item:{item}", credentials.Value!.ItemId ?? "null");

        return Task.FromResult(credentials.Value!);
    }

    public Task<bool> IsLoggedIn()
    {
        CheckCredentials();

        var result = credentials.Value.AccessToken is not null;

        logger.LogInformation("IsLoggedIn: OK {result}", result);

        return Task.FromResult(result);
    }

    public Task LogOut()
    {
        CheckCredentials();

        credentials.Value.AccessToken = null;

        logger.LogInformation("LogOut: OK");

        return Task.CompletedTask;
    }

    private void CheckCredentials()
    {
        if (credentials == null || credentials.Value == null || credentials.Value.Products == null)
        {
            throw new ArgumentNullException(nameof(credentials), "Please supply Plaid credentials in .NET configuration");
        }
    }

    private PlaidServiceException Error(PlaidError error, [CallerMemberName] string callerName = "")
    {
        var result = new PlaidServiceException((int)HttpStatusCode.BadRequest, error);
        logger.LogError(result, "{caller}: FAILED", callerName);

        return result;
    }
}
