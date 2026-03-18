using System.Net;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using OptiPowerTools.Hangfire.Configuration;

namespace OptiPowerTools.Hangfire.Cms;

/// <summary>
/// Middleware that serves the Hangfire dashboard embedded in the Optimizely CMS shell.
/// Uses an inline HTML page with an iframe, removing the dependency on MVC controller routing.
/// </summary>
internal sealed class HangfireCmsMiddleware
{
    private readonly RequestDelegate _next;
    private readonly OptiPowerToolHangfireOptions _options;

    public HangfireCmsMiddleware(RequestDelegate next, IOptions<OptiPowerToolHangfireOptions> options)
    {
        _next = next;
        _options = options.Value;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        if (!IsShellPageRequest(context))
        {
            await _next(context);
            return;
        }

        if (context.User.Identity?.IsAuthenticated != true
            || _options.AuthorizedRoles is not { } roles
            || !roles.Any(role => context.User.IsInRole(role)))
        {
            context.Response.StatusCode = (int)HttpStatusCode.Forbidden;
            return;
        }

        var html = BuildShellPage();
        context.Response.ContentType = "text/html; charset=utf-8";
        await context.Response.WriteAsync(html, Encoding.UTF8);
    }

    private bool IsShellPageRequest(HttpContext context) =>
        context.Request.Method == HttpMethods.Get
        && context.Request.Path.Equals(_options.CmsShellPath, StringComparison.OrdinalIgnoreCase);

    private string BuildShellPage()
    {
        var dashboardPath = WebUtility.HtmlEncode(_options.DashboardPath);
        var title = WebUtility.HtmlEncode(_options.DashboardTitle);

        return $$"""
            <!DOCTYPE html>
            <html lang="en">
            <head>
                <meta charset="utf-8" />
                <title>{{title}}</title>
                <style>
                    html, body, .iframe-container {
                        height: 100%;
                        margin: 0;
                        padding: 0;
                        overflow: hidden;
                    }
                    iframe {
                        width: 100%;
                        height: 100%;
                        border: none;
                    }
                </style>
            </head>
            <body>
                <div class="iframe-container">
                    <iframe src="{{dashboardPath}}" title="{{title}}">
                        <p>Your browser does not support iframes.</p>
                    </iframe>
                </div>
            </body>
            </html>
            """;
    }
}
