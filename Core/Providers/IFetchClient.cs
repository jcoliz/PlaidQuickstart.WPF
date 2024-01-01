using Core.Models;

namespace Core.Providers;

/// <summary>
/// Defines a service used to fetch data from the Plaid Service
/// </summary>
public interface IFetchClient
{
    /// <summary>
    /// Fetch balances
    /// </summary>
    /// <returns>Formatted data ready for display</returns>
    Task<WireDataTable> Balance();

    /// <summary>
    /// Fetch transactions
    /// </summary>
    /// <returns>Formatted data ready for display</returns>
    Task<WireDataTable> Transactions();

    /// <summary>
    /// Fetch institutions
    /// </summary>
    /// <returns>Formatted data ready for display</returns>
    Task<WireDataTable> Institutions();
}
