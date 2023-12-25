using Core.Models;

namespace Core.Providers;

/// <summary>
/// Defines a service to communicate with Plaid Link
/// </summary>
public interface ILinkClient
{
    Task<string> CreateLinkToken(bool? fix);

    Task<PlaidCredentials> ExchangePublicToken(LinkResult link);

    Task<PlaidCredentials> Info();
}
