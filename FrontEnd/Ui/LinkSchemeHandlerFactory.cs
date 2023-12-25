﻿using CefSharp;
using Core.Models;
using Core.Providers;
using Microsoft.Extensions.Logging;
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
public class LinkSchemeHandlerFactory(ILogger<LinkSchemeHandlerFactory> logger, LinkResourceHandler linkResourceHandler) : ISchemeHandlerFactory
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

        // Is this an "/api" call?
        if (fileName.StartsWith("/api/"))
        {
            return linkResourceHandler;
        }

        logger.LogError("URL not found {url}", uri);
        return ResourceHandler.ForErrorMessage("URL Not found", HttpStatusCode.NotFound);
    }
}

/// <summary>
/// Serve resources for Link API calls
/// </summary>
/// <param name="linkclient">Service providing the actual calls to Plaid Link servers</param>
public class LinkResourceHandler(ILogger<LinkResourceHandler> logger, ILinkClient linkclient) : ResourceHandler
{
    public override CefReturnValue ProcessRequestAsync(IRequest request, ICallback callback)
    {
        var uri = new Uri(request.Url);
        var segments = uri.AbsolutePath.ToLowerInvariant().Split('/');
        var endpoint = segments[2];

        _ = Task.Run(async () =>
        {
            using (callback)
            {
                try
                {
                    T getpostdata<T>()
                    {
                        var postDataElement = request.PostData.Elements.FirstOrDefault()!.Bytes;
                        var str = (postDataElement is not null) ? Encoding.UTF8.GetString(postDataElement) : null;
                        return JsonSerializer.Deserialize<T>(str!)!;
                    }

                    var query = HttpUtility.ParseQueryString(uri.Query);

                    T? getquery<T>(string key)
                    {
                        var val = query[key];
                        if (val is not null)
                            return (T)Convert.ChangeType(val, typeof(T));
                        else
                            return default(T);
                    }

                    object? response = endpoint switch
                    {
                        "create_link_token" => await linkclient.CreateLinkToken(fix: getquery<bool>("fix")),
                        "exchange_public_token" => await linkclient.ExchangePublicToken(getpostdata<LinkResult>()),
                        "info" => await linkclient.Info(),
                        _ => throw new NotImplementedException()
                    };

                    var json = JsonSerializer.Serialize(response);
                    var stream = GetMemoryStream(json, Encoding.UTF8);

                    //Reset the stream position to 0 so the stream can be copied into the underlying unmanaged buffer
                    stream.Position = 0;

                    //Populate the response values - No longer need to implement GetResponseHeaders (unless you need to perform a redirect)
                    ResponseLength = stream.Length;
                    MimeType = "application/json";
                    StatusCode = (int)HttpStatusCode.OK;
                    Stream = stream;

                    logger.LogInformation("ProcessRequestAsync: OK {endpoint}", endpoint);

                    callback.Continue();
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "ProcessRequestAsync: FAILED");

                    ResponseLength = 0;
                    MimeType = "application/json";
                    StatusCode = (int)HttpStatusCode.InternalServerError;

                    callback.Continue();
                }
            }
        });

        return CefReturnValue.ContinueAsync;
    }
}
