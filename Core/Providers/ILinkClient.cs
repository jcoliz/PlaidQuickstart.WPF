using Core.Models;

namespace Core.Providers;

/// <summary>
/// Defines a service to communicate with Plaid Link
/// </summary>
public interface ILinkClient
{
    Task<PlaidCredentials> Info();
}
