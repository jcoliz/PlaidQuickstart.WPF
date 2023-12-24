using CefSharp;
using Microsoft.Extensions.Logging;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Web;

using Application = System.Windows.Application;

namespace FrontEnd.Ui;

/// <summary>
/// Scheme handler to intercept CefSharp links, and provide local responses
/// </summary>
/// <remarks>
/// Should not be enabled in production. In production, just serve resources from app service.
/// </remarks>
/// <param name="linkResourceHandler">Service to be returned in the case of link API call</param>
public class LinkSchemeHandlerFactory(ILogger<LinkSchemeHandlerFactory> logger) : ISchemeHandlerFactory
{
    public static string Scheme => "https";
    public static string Host => "localhost";

    public IResourceHandler Create(IBrowser browser, IFrame frame, string schemeName, IRequest request)
    {
        var uri = new Uri(request.Url);
        var fileName = uri.AbsolutePath.ToLowerInvariant();

        //
        // Is this a static file?
        //
        // Static files are located in `wwwroot` directory, and added to the project as
        // embedded resources
        //
        var resources = Application.ResourceAssembly.GetManifestResourceNames();
        var found = resources.Where(x => x.EndsWith("wwwroot" + uri.AbsolutePath.Replace('/', '.')));
        if (found.Any())
        {
            return ResourceHandler.FromStream(Application.ResourceAssembly.GetManifestResourceStream(found.First()));
        }

        logger.LogError("URL not found {url}", uri);
        return ResourceHandler.ForErrorMessage("URL Not found", HttpStatusCode.NotFound);
    }
}
