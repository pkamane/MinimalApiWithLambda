using Microsoft.EntityFrameworkCore;
using MinimalApiWithLambda.Core.Data;
using MinimalApiWithLambda.Core.IRepository;
using MinimalApiWithLambda.Core.Models;

namespace MinimalApiWithLambda.Core.Repository;

public sealed class TodoRepository : ITodoRepository
{
    private readonly AppDbContext _dbContext;

    public TodoRepository(AppDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await _dbContext.TodoItems
            .AsNoTracking()
            .OrderBy(x => x.Id)
            .ToListAsync(cancellationToken);
    }

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        return _dbContext.TodoItems
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    public async Task<TodoItem> AddAsync(TodoItem item, CancellationToken cancellationToken = default)
    {
        _dbContext.TodoItems.Add(item);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return item;
    }

    public async Task<bool> MarkCompletedAsync(int id, CancellationToken cancellationToken = default)
    {
        var rows = await _dbContext.TodoItems
            .Where(x => x.Id == id)
            .ExecuteUpdateAsync(
                setters => setters.SetProperty(x => x.IsCompleted, true),
                cancellationToken);

        return rows > 0;
    }
}
