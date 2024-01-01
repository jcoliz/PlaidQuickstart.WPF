using Core.Models;

namespace Core.Providers;

/// <summary>
/// Defines a service to communicate with Plaid Link
/// </summary>
public interface ILinkClient
{
    /// <summary>
    /// Create Link Token
    /// </summary>
    /// <remarks>
    /// Creates a link_token, which is required as a parameter when initializing Link. 
    /// </remarks>
    /// <seealso href="https://plaid.com/docs/api/tokens/#linktokencreate"/>
    /// <returns>Newly created token</returns>
    Task<string> CreateLinkToken();

    /// <summary>
    /// Exchange public token for an access token
    /// </summary>
    /// <remarks>
    /// Exchange a Link public_token for an API access_token. Link hands off the public_token 
    /// client-side via the onSuccess callback once a user has successfully created an Item. 
    /// The public_token is ephemeral and expires after 30 minutes. An access_token does not 
    /// expire, but can be revoked by calling /item/remove.
    ///
    /// The response also includes an item_id that should be stored with the access_token.
    /// The item_id is used to identify an Item in a webhook.The item_id can also be 
    /// retrieved by making an /item/get request.
    /// </remarks>
    /// <seealso href="https://plaid.com/docs/api/tokens/#itempublic_tokenexchange"/>
    /// <param name="link">Result of the preceding Link flow</param>
    /// <returns>Full set of credentials</returns>
    Task<PlaidCredentials> ExchangePublicToken(LinkResult link);

    /// <summary>
    /// Retrieve the current plaid credentials
    /// </summary>
    /// <returns>Full set of credentials</returns>
    Task<PlaidCredentials> Info();

    /// <summary>
    /// Find out if current user is logged in
    /// </summary>
    /// <returns>true if current user is logged in</returns>
    Task<bool> IsLoggedIn();

    /// <summary>
    /// Log current user out (internally)
    /// </summary>
    /// <remarks>
    /// Simply removes the current item and access token from local storage
    /// </remarks>
    /// <returns></returns>
    Task LogOut();
}
