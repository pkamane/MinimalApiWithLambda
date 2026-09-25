using Microsoft.EntityFrameworkCore;
using MinimalApiWithLambda.Core.Models;

namespace MinimalApiWithLambda.Core.Data;

public sealed class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.ToTable("todo_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Title).HasMaxLength(200).IsRequired();
            entity.Property(x => x.IsCompleted).IsRequired();
            entity.Property(x => x.CreatedAtUtc).IsRequired();
        });
    }
}
