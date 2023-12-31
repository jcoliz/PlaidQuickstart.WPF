using Core.Models;

namespace Core.Providers;

/// <summary>
/// Defines a service to communicate with Plaid Link
/// </summary>
public interface ILinkClient
{
    Task<string> CreateLinkToken();

    Task<PlaidCredentials> ExchangePublicToken(LinkResult link);

    Task<PlaidCredentials> Info();

    Task<bool> IsLoggedIn();
}
