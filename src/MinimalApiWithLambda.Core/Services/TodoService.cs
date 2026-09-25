using MinimalApiWithLambda.Core.Dto;
using MinimalApiWithLambda.Core.IRepository;
using MinimalApiWithLambda.Core.IServices;
using MinimalApiWithLambda.Core.Models;

namespace MinimalApiWithLambda.Core.Services;

public sealed class TodoService : ITodoService
{
    private readonly ITodoRepository _todoRepository;

    public TodoService(ITodoRepository todoRepository)
    {
        _todoRepository = todoRepository;
    }

    public async Task<IReadOnlyList<TodoItemDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var items = await _todoRepository.GetAllAsync(cancellationToken);
        return items.Select(Map).ToList();
    }

    public async Task<TodoItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var item = await _todoRepository.GetByIdAsync(id, cancellationToken);
        return item is null ? null : Map(item);
    }

    public async Task<TodoItemDto> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Title))
        {
            throw new ArgumentException("Title is required.", nameof(request));
        }

        var item = new TodoItem
        {
            Title = request.Title.Trim(),
            IsCompleted = false,
            CreatedAtUtc = DateTime.UtcNow
        };

        var created = await _todoRepository.AddAsync(item, cancellationToken);
        return Map(created);
    }

    public Task<bool> CompleteAsync(int id, CancellationToken cancellationToken = default)
    {
        return _todoRepository.MarkCompletedAsync(id, cancellationToken);
    }

    private static TodoItemDto Map(TodoItem item) =>
        new(item.Id, item.Title, item.IsCompleted, item.CreatedAtUtc);
}
