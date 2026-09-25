using MinimalApiWithLambda.Core.Dto;

namespace MinimalApiWithLambda.Core.IServices;

public interface ITodoService
{
    Task<IReadOnlyList<TodoItemDto>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<TodoItemDto?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoItemDto> CreateAsync(CreateTodoRequest request, CancellationToken cancellationToken = default);
    Task<bool> CompleteAsync(int id, CancellationToken cancellationToken = default);
}
