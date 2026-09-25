using MinimalApiWithLambda.Core.Models;

namespace MinimalApiWithLambda.Core.IRepository;

public interface ITodoRepository
{
    Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task<bool> MarkCompletedAsync(int id, CancellationToken cancellationToken = default);
}
