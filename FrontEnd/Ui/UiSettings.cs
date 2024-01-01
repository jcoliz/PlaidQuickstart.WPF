namespace FrontEnd.Ui;

/// <summary>
/// Settings which affect how the UI is presented
/// </summary>
public class UiSettings
{
    public static string Section => "Ui";

    /// <summary>
    /// Where to look for the Link web page
    /// </summary>
    public string? WebAddress { get; set; }
}
