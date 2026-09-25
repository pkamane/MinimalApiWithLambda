using MinimalApiWithLambda.Api.Endpoints;
using MinimalApiWithLambda.Lambda.DI;

Directory.SetCurrentDirectory(AppContext.BaseDirectory);

_ = LambdaHost.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new() { Title = "MinimalApiWithLambda", Version = "v1" });
});

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI();

app.MapGet("/health", () => Results.Ok(new { status = "Healthy", timeUtc = DateTime.UtcNow }))
    .WithName("HealthCheck");

app.MapTodoEndpoints();

app.Run();
