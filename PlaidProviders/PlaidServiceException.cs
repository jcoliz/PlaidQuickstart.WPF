using Core.Models;
using Going.Plaid.Entity;

namespace PlaidProviders;

/// <summary>
/// Represents a failed attempt to interact with Plaid Services
/// </summary>
public class PlaidServiceException : Exception
{
    /// <summary>
    /// HTTP Status code
    /// </summary>
    public int Status
    {
        get; private set;
    }

    /// <summary>
    /// Details of error returned from Plaid Service
    /// </summary>
    public WireError? Error
    {
        get; private set;
    }

    public PlaidServiceException(int status) : base($"Fetch error {status}")
    {
        Status = status;
    }
    public PlaidServiceException(int status, WireError error)
        : base($"Fetch error {status} {error.error_message}")
    {
        Status = status;
        Error = error;
    }

    public PlaidServiceException(int status, PlaidError error)
    : base($"Fetch error {status} {error.ErrorMessage}")
    {
        Status = status;
        Error = ConvertFromPlaidError(error);
    }

    private static WireError ConvertFromPlaidError(PlaidError source)
    {
        var result = new WireError();

        try
        {
            result.error_message = source.ErrorMessage;
            result.display_message = source.DisplayMessage;

            result.error_type = source.ErrorType;
            result.error_code = source.ErrorCode;

            result.error_type_path = _error_type_paths.GetValueOrDefault(result.error_type);
        }
        catch
        {
            // If we run into errors here, we'll just take as much as we have converted sofar
        }

        return result;
    }

    private static readonly Dictionary<string, string> _error_type_paths = new()
    {
        { "ITEM_ERROR", "item" },
        { "INSTITUTION_ERROR", "institution" },
        { "API_ERROR", "api" },
        { "ASSET_REPORT_ERROR", "assets" },
        { "BANK_TRANSFER_ERROR", "bank-transfers" },
        { "INVALID_INPUT", "invalid-input" },
        { "INVALID_REQUEST", "invalid-request" },
        { "INVALID_RESULT", "invalid-result" },
        { "OAUTH_ERROR", "oauth" },
        { "PAYMENT_ERROR", "payment" },
        { "RATE_LIMIT_EXCEEDED", "rate-limit-exceeded" },
        { "RECAPTCHA_ERROR", "recaptcha" },
        { "SANDBOX_ERROR", "sandbox" },
    };
}