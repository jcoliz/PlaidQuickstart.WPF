using System.Net;
using CefSharp;
using Microsoft.Extensions.Logging;
using Application = System.Windows.Application;

namespace FrontEnd.Ui;

/// <summary>
/// Scheme handler to intercept CefSharp links, and provide local responses
/// </summary>
/// <remarks>
/// Should not be enabled in production. In production, just serve resources from app service.
/// </remarks>
public class LinkSchemeHandlerFactory(ILogger<LinkSchemeHandlerFactory> logger) : ISchemeHandlerFactory
{
    public static string Scheme => "https";
    public static string Host => "standalone";

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
            logger.LogDebug("Scheme Handler: Supplying static file {uri}", uri);
            return ResourceHandler.FromStream(Application.ResourceAssembly.GetManifestResourceStream(found.First()));
        }

        // If not, we don't know how to serve that
        logger.LogError("Scheme Handler: URL not found {url}", uri);
        return ResourceHandler.ForErrorMessage("URL Not found", HttpStatusCode.NotFound);
    }
}
