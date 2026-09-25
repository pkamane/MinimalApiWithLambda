namespace MinimalApiWithLambda.Core.Configuration;

public sealed class AppSettings
{
    public ConnectionStringSettings ConnectionStrings { get; set; } = new();
}

public sealed class ConnectionStringSettings
{
    public string DefaultConnection { get; set; } = "Data Source=todos.db";
}
