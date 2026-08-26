using System.Net.Http.Headers;

namespace EndustriB2C.Middleware;

public static class DevSpaProxy
{
    private const string SpaBase = "https://localhost:44408";
    private static readonly HttpClient Http = CreateClient();

    private static HttpClient CreateClient()
    {
        var handler = new HttpClientHandler
        {
            AllowAutoRedirect = false,
            UseCookies = false,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true
        };
        return new HttpClient(handler) { Timeout = TimeSpan.FromMinutes(2) };
    }

    public static async Task WriteAsync(HttpContext context)
    {
        var target = SpaBase + context.Request.Path + context.Request.QueryString;
        using var request = new HttpRequestMessage(new HttpMethod(context.Request.Method), target);

        if (context.Request.ContentLength > 0 && !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
        {
            request.Content = new StreamContent(context.Request.Body);
            if (context.Request.ContentType is not null)
            {
                request.Content.Headers.ContentType = MediaTypeHeaderValue.Parse(context.Request.ContentType);
            }
        }

        foreach (var header in context.Request.Headers)
        {
            if (header.Key.Equals("Host", StringComparison.OrdinalIgnoreCase)) continue;
            if (!request.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray()) && request.Content is not null)
            {
                request.Content.Headers.TryAddWithoutValidation(header.Key, header.Value.ToArray());
            }
        }

        using var response = await Http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
        var wantsHtml = context.Request.Headers.Accept.ToString().Contains("text/html", StringComparison.OrdinalIgnoreCase);
        if ((int)response.StatusCode == 404 && HttpMethods.IsGet(context.Request.Method) && wantsHtml)
        {
            using var indexRequest = new HttpRequestMessage(HttpMethod.Get, SpaBase + "/");
            using var indexResponse = await Http.SendAsync(indexRequest, HttpCompletionOption.ResponseHeadersRead, context.RequestAborted);
            await CopyAsync(context, indexResponse);
            return;
        }

        await CopyAsync(context, response);
    }

    private static async Task CopyAsync(HttpContext context, HttpResponseMessage response)
    {
        context.Response.StatusCode = (int)response.StatusCode;
        foreach (var header in response.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        foreach (var header in response.Content.Headers)
        {
            context.Response.Headers[header.Key] = header.Value.ToArray();
        }
        context.Response.Headers.Remove("transfer-encoding");
        await response.Content.CopyToAsync(context.Response.Body, context.RequestAborted);
    }
}
