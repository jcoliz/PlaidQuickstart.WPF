using Core.Models;
using Core.Providers;
using Going.Plaid;
using Going.Plaid.Entity;
using Going.Plaid.Item;
using Going.Plaid.Link;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
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
public class LinkClient(ILogger<LinkClient> logger, IOptions<PlaidCredentials> credentials) : ILinkClient
{
    public Task<PlaidCredentials> Info()
    {
        if (credentials == null || credentials.Value == null || credentials.Value.Products == null)
            throw new ArgumentNullException(nameof(credentials), "Please supply Plaid credentials in .NET configuration");

        logger.LogInformation("Info: OK item:{item}", credentials.Value!.ItemId ?? "null");

        return Task.FromResult(credentials.Value!);
    }
}
