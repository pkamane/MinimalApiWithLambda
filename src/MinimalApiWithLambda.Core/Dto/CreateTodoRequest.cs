namespace MinimalApiWithLambda.Core.Dto;

public sealed class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
}
