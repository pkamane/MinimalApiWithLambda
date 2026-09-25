using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MinimalApiWithLambda.Core.Configuration;
using MinimalApiWithLambda.Core.Data;
using MinimalApiWithLambda.Core.IRepository;
using MinimalApiWithLambda.Core.IServices;
using MinimalApiWithLambda.Core.Repository;
using MinimalApiWithLambda.Core.Services;

namespace MinimalApiWithLambda.Lambda.DI;

/// <summary>
/// Central composition root for Lambda handlers and the Minimal API adapter.
/// One resolver builds a single root ServiceProvider,
/// then handlers create a scope per request for safe DbContext lifetime.
/// </summary>
public sealed class DependencyResolver
{
    public IServiceProvider ServiceProvider { get; }
    public AppSettings Settings { get; private set; } = new();
    public Action<IServiceCollection>? RegisterServices { get; }

    public DependencyResolver(Action<IServiceCollection>? registerServices = null)
    {
        RegisterServices = registerServices;

        var services = new ServiceCollection();
        ConfigureServices(services);
        ServiceProvider = services.BuildServiceProvider();

        EnsureDatabaseCreated();
    }

    private void ConfigureServices(IServiceCollection services)
    {
        var environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Development";
        var connectionFromEnv = Environment.GetEnvironmentVariable("DATABASE_CONNECTION_STRING");

        IConfiguration configuration;
        var configurationBuilder = new ConfigurationBuilder();

        if (!string.IsNullOrWhiteSpace(connectionFromEnv))
        {
            // Container / production-style configuration via environment variables.
            Settings = new AppSettings
            {
                ConnectionStrings = new ConnectionStringSettings
                {
                    DefaultConnection = connectionFromEnv
                }
            };

            configurationBuilder.AddEnvironmentVariables();
            configuration = configurationBuilder.Build();
        }
        else if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
        {
            // Local development: appsettings.core.json copied to output directory.
            configurationBuilder
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.core.json", optional: false, reloadOnChange: false);

            configuration = configurationBuilder.Build();
            Settings = configuration.GetSection("Settings").Get<AppSettings>() ?? new AppSettings();
        }
        else
        {
            throw new InvalidOperationException(
                "Set DATABASE_CONNECTION_STRING for non-development environments, " +
                "or run with ASPNETCORE_ENVIRONMENT=Development and appsettings.core.json.");
        }

        services.AddSingleton<IConfiguration>(configuration);
        services.AddSingleton(Settings);
        services.Configure<AppSettings>(configuration.GetSection("Settings"));

        services.AddLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
            logging.SetMinimumLevel(LogLevel.Information);
        });

        // Pooled factory keeps allocations low under concurrent Lambda invocations.
        services.AddPooledDbContextFactory<AppDbContext>(options =>
        {
            options.UseSqlite(Settings.ConnectionStrings.DefaultConnection);

            if (string.Equals(environment, "Development", StringComparison.OrdinalIgnoreCase))
            {
                options.EnableDetailedErrors();
            }
        }, poolSize: 64);

        // Scoped DbContext: resolve one context per request scope (see LambdaHost.CreateScope()).
        services.AddScoped(sp =>
            sp.GetRequiredService<IDbContextFactory<AppDbContext>>().CreateDbContext());

        services.AddScoped<ITodoRepository, TodoRepository>();
        services.AddScoped<ITodoService, TodoService>();

        RegisterServices?.Invoke(services);
    }

    private void EnsureDatabaseCreated()
    {
        using var scope = ServiceProvider.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        dbContext.Database.EnsureCreated();
    }
}
