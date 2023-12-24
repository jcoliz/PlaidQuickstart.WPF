using System.ComponentModel;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace FrontEnd.Ui;

/// <summary>
/// View Model for the main interaction window
/// </summary>
/// <param name="fetchClient">Client we will use to fetch bank data</param>
public class MainViewModel(ILogger<MainViewModel> logger, IOptions<UiSettings> settings)
{
    /// <summary>
    /// Web location of the home page
    /// </summary>
    public Uri? WebAddress
    {
        get
        {
            var web_address = settings.Value?.WebAddress;

            if (web_address is null)
            {
                logger.LogError("Must set WebAddress in UI settings");
                return null;
            }
            else
            {
                var result = new Uri(web_address);
                logger.LogInformation("Using web address {uri}", result);
                return result;
            }
        }
    }
}
