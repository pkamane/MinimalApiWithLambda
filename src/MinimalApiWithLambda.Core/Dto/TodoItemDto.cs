namespace MinimalApiWithLambda.Core.Dto;

public sealed record TodoItemDto(int Id, string Title, bool IsCompleted, DateTime CreatedAtUtc);
