namespace Core.Models;

public class PlaidCredentials
{
	public static string SectionKey => "Plaid";
	public string? LinkToken { get; set; }
	public string? AccessToken { get; set; }
	public string? ItemId { get; set; }
	public string? Products { get; set; }
	public string? CountryCodes { get; set; }
    public string? Language { get; set; }
}
