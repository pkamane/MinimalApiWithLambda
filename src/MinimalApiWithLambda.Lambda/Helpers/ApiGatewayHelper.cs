using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;

namespace MinimalApiWithLambda.Lambda.Helpers;

public static class ApiGatewayHelper
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public static APIGatewayProxyResponse CreateResponse(int statusCode, object? body = null)
    {
        return new APIGatewayProxyResponse
        {
            StatusCode = statusCode,
            Headers = new Dictionary<string, string>
            {
                ["Content-Type"] = "application/json"
            },
            Body = body is null ? null : JsonSerializer.Serialize(body, JsonOptions)
        };
    }

    public static APIGatewayProxyResponse Ok(object body) =>
        CreateResponse((int)HttpStatusCode.OK, body);

    public static APIGatewayProxyResponse BadRequest(string message) =>
        CreateResponse((int)HttpStatusCode.BadRequest, new { error = message });

    public static APIGatewayProxyResponse NotFound(string message) =>
        CreateResponse((int)HttpStatusCode.NotFound, new { error = message });

    public static APIGatewayProxyResponse InternalServerError(string message) =>
        CreateResponse((int)HttpStatusCode.InternalServerError, new { error = message });
}
