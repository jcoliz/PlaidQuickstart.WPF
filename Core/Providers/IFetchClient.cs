using Core.Models;

namespace Core.Providers;

/// <summary>
/// Defines a service used to fetch data from the Plaid Service
/// </summary>
public interface IFetchClient
{
    Task<WireDataTable> Balance();
    Task<WireDataTable> Transactions();
}
