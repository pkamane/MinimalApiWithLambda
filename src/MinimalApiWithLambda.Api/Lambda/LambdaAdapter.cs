using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.AspNetCore.Http;

namespace MinimalApiWithLambda.Api.Lambda;

public static class LambdaAdapter
{
    public static async Task<APIGatewayProxyRequest> ToApiGatewayRequestAsync(HttpRequest http, string resourceTemplate)
    {
        string? body = null;

        http.EnableBuffering();
        if (http.Body.CanRead)
        {
            using var reader = new StreamReader(http.Body, leaveOpen: true);
            body = await reader.ReadToEndAsync();
            http.Body.Position = 0;
        }

        return new APIGatewayProxyRequest
        {
            HttpMethod = http.Method,
            Path = http.Path,
            Resource = resourceTemplate,
            Headers = http.Headers.ToDictionary(h => h.Key, h => h.Value.ToString(), StringComparer.OrdinalIgnoreCase),
            QueryStringParameters = http.Query.ToDictionary(q => q.Key, q => q.Value.ToString(), StringComparer.OrdinalIgnoreCase),
            PathParameters = http.RouteValues.ToDictionary(kv => kv.Key, kv => kv.Value?.ToString() ?? string.Empty, StringComparer.OrdinalIgnoreCase),
            Body = string.IsNullOrWhiteSpace(body) ? null : body
        };
    }

    public static async Task<IResult> ExecuteAsync(
        Func<APIGatewayProxyRequest, ILambdaContext, Task<APIGatewayProxyResponse>> handler,
        HttpContext ctx,
        string resourceTemplate)
    {
        var request = await ToApiGatewayRequestAsync(ctx.Request, resourceTemplate);
        var response = await handler(request, new LambdaContextStub());

        if (response.Headers != null)
        {
            foreach (var header in response.Headers)
            {
                ctx.Response.Headers[header.Key] = header.Value;
            }
        }

        return Results.Text(response.Body ?? string.Empty, "application/json", statusCode: response.StatusCode);
    }
}
