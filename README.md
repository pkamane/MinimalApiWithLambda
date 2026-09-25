# MinimalApiWithLambda

Standalone proof-of-concept: share Lambda-style handlers between an AWS Lambda library and an ASP.NET Core Minimal API host.

## Projects

| Project | Purpose |
|---------|---------|
| `MinimalApiWithLambda.Core` | Todo domain, EF Core, repositories, services |
| `MinimalApiWithLambda.Lambda` | `DependencyResolver`, `LambdaHost`, handlers |
| `MinimalApiWithLambda.Api` | Minimal API + `LambdaAdapter` bridge |

## DependencyResolver

1. `DependencyResolver` builds the root `IServiceProvider` once.
2. `LambdaHost` reuses it across warm invocations.
3. Handlers call `LambdaHost.CreateScope()` per request for safe `DbContext` lifetime.
4. `IDbContextFactory<AppDbContext>` is pooled; each scope creates one context.

Configuration:

- `DATABASE_CONNECTION_STRING` environment variable, or
- `appsettings.core.json` when `ASPNETCORE_ENVIRONMENT=Development`

## Run

```bash
cd src/MinimalApiWithLambda.Api
dotnet run
```

Swagger: `http://localhost:5000/swagger`

```bash
curl -X POST http://localhost:5000/api/todos -H "Content-Type: application/json" -d "{\"title\":\"Buy milk\"}"
curl http://localhost:5000/api/todos
curl -X POST http://localhost:5000/api/todos/1/complete
```

SQLite file `todos.db` is created automatically on first run.

## Public GitHub

No production credentials. SQLite only. Local DB files are gitignored.
