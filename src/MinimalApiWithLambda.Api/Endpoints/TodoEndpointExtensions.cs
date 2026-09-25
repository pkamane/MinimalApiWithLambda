using MinimalApiWithLambda.Api.Lambda;
using MinimalApiWithLambda.Lambda.Functions;

namespace MinimalApiWithLambda.Api.Endpoints;

public static class TodoEndpointExtensions
{
    public static WebApplication MapTodoEndpoints(this WebApplication app)
    {
        const string basePath = "/api/todos";
        var todoFunctions = new TodoFunctions();

        app.MapGet(basePath, ctx =>
                LambdaAdapter.ExecuteAsync(todoFunctions.ListTodos, ctx, basePath))
            .WithName("ListTodos")
            .WithSummary("List todos (Lambda adapter -> TodoFunctions.ListTodos)");

        app.MapGet($"{basePath}/{{id:int}}", ctx =>
                LambdaAdapter.ExecuteAsync(todoFunctions.GetTodo, ctx, $"{basePath}/{{id}}"))
            .WithName("GetTodo")
            .WithSummary("Get todo by id (Lambda adapter -> TodoFunctions.GetTodo)");

        app.MapPost(basePath, ctx =>
                LambdaAdapter.ExecuteAsync(todoFunctions.CreateTodo, ctx, basePath))
            .WithName("CreateTodo")
            .WithSummary("Create todo (Lambda adapter -> TodoFunctions.CreateTodo)");

        app.MapPost($"{basePath}/{{id:int}}/complete", ctx =>
                LambdaAdapter.ExecuteAsync(todoFunctions.CompleteTodo, ctx, $"{basePath}/{{id}}/complete"))
            .WithName("CompleteTodo")
            .WithSummary("Mark todo complete (Lambda adapter -> TodoFunctions.CompleteTodo)");

        return app;
    }
}
