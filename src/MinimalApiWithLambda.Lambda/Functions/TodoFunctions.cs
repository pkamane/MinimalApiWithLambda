using System.Net;
using System.Text.Json;
using Amazon.Lambda.APIGatewayEvents;
using Amazon.Lambda.Core;
using Microsoft.Extensions.DependencyInjection;
using MinimalApiWithLambda.Core.Dto;
using MinimalApiWithLambda.Core.IServices;
using MinimalApiWithLambda.Lambda.DI;
using MinimalApiWithLambda.Lambda.Helpers;

namespace MinimalApiWithLambda.Lambda.Functions;

/// <summary>
/// Lambda-style handlers invoked directly by AWS Lambda or via the Minimal API adapter.
/// Each method creates its own DI scope so DbContext is never shared across requests.
/// </summary>
public sealed class TodoFunctions
{
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<APIGatewayProxyResponse> ListTodos(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            using var scope = LambdaHost.CreateScope();
            var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
            var items = await todoService.GetAllAsync();
            return ApiGatewayHelper.Ok(new { items });
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"ListTodos failed: {ex.Message}");
            return ApiGatewayHelper.InternalServerError(ex.Message);
        }
    }

    public async Task<APIGatewayProxyResponse> GetTodo(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (!TryGetTodoId(request, out var id))
            {
                return ApiGatewayHelper.BadRequest("Invalid todo id.");
            }

            using var scope = LambdaHost.CreateScope();
            var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
            var item = await todoService.GetByIdAsync(id);
            if (item is null)
            {
                return ApiGatewayHelper.NotFound($"Todo {id} was not found.");
            }

            return ApiGatewayHelper.Ok(item);
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"GetTodo failed: {ex.Message}");
            return ApiGatewayHelper.InternalServerError(ex.Message);
        }
    }

    public async Task<APIGatewayProxyResponse> CreateTodo(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Body))
            {
                return ApiGatewayHelper.BadRequest("Request body is required.");
            }

            var payload = JsonSerializer.Deserialize<CreateTodoRequest>(request.Body, JsonOptions);
            if (payload is null || string.IsNullOrWhiteSpace(payload.Title))
            {
                return ApiGatewayHelper.BadRequest("Title is required.");
            }

            using var scope = LambdaHost.CreateScope();
            var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
            var created = await todoService.CreateAsync(payload);
            return ApiGatewayHelper.CreateResponse((int)HttpStatusCode.Created, created);
        }
        catch (ArgumentException ex)
        {
            return ApiGatewayHelper.BadRequest(ex.Message);
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"CreateTodo failed: {ex.Message}");
            return ApiGatewayHelper.InternalServerError(ex.Message);
        }
    }

    public async Task<APIGatewayProxyResponse> CompleteTodo(APIGatewayProxyRequest request, ILambdaContext context)
    {
        try
        {
            if (!TryGetTodoId(request, out var id))
            {
                return ApiGatewayHelper.BadRequest("Invalid todo id.");
            }

            using var scope = LambdaHost.CreateScope();
            var todoService = scope.ServiceProvider.GetRequiredService<ITodoService>();
            var updated = await todoService.CompleteAsync(id);
            if (!updated)
            {
                return ApiGatewayHelper.NotFound($"Todo {id} was not found.");
            }

            return ApiGatewayHelper.Ok(new { id, isCompleted = true });
        }
        catch (Exception ex)
        {
            context.Logger.LogLine($"CompleteTodo failed: {ex.Message}");
            return ApiGatewayHelper.InternalServerError(ex.Message);
        }
    }

    private static bool TryGetTodoId(APIGatewayProxyRequest request, out int id)
    {
        id = 0;
        if (request.PathParameters != null &&
            request.PathParameters.TryGetValue("id", out var idValue) &&
            int.TryParse(idValue, out id))
        {
            return true;
        }

        return false;
    }
}
