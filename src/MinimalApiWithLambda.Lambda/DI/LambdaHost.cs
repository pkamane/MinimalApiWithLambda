using Microsoft.Extensions.DependencyInjection;

namespace MinimalApiWithLambda.Lambda.DI;

/// <summary>
/// Process-wide singleton service provider for warm Lambda containers and the Minimal API host.
/// </summary>
public static class LambdaHost
{
    private static readonly Lazy<IServiceProvider> LazyProvider = new(
        () => new DependencyResolver().ServiceProvider,
        LazyThreadSafetyMode.ExecutionAndPublication);

    public static IServiceProvider Services => LazyProvider.Value;

    public static IServiceScope CreateScope() => Services.CreateScope();
}
