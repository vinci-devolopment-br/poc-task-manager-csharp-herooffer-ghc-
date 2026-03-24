using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Data;
using TaskManager.Web.Models;

namespace TaskManager.Web.Repositories;

/// <summary>
/// Implementação do repositório de tarefas usando Entity Framework Core
/// </summary>
public class TaskRepository : ITaskRepository
{
    private readonly TaskManagerDbContext _context;

    public TaskRepository(TaskManagerDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TaskItem>> GetAllAsync(int page = 1, int pageSize = 2, bool asc = false)
    {
        var query = _context.Tasks.AsQueryable();
        query = asc ? query.OrderBy(t => t.CreatedAt) : query.OrderByDescending(t => t.CreatedAt);
        return await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }

    public async Task<TaskItem?> GetByIdAsync(long id)
    {
        return await _context.Tasks.FindAsync(id);
    }

    public async Task<TaskItem> CreateAsync(TaskItem task)
    {
        _context.Tasks.Add(task);
        await _context.SaveChangesAsync();
        return task;
    }

    public async Task<TaskItem?> UpdateAsync(TaskItem task)
    {
        var existing = await _context.Tasks.FindAsync(task.Id);
        if (existing == null)
            return null;

        _context.Entry(existing).CurrentValues.SetValues(task);
        await _context.SaveChangesAsync();
        return existing;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        var task = await _context.Tasks.FindAsync(id);
        if (task == null)
            return false;

        _context.Tasks.Remove(task);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<int> CountAllAsync()
    {
        return await _context.Tasks.CountAsync();
    }

    public async Task<int> CountCompletedAsync()
    {
        return await _context.Tasks.CountAsync(t => t.Completed);
    }

    public async Task<int> CountPendingAsync()
    {
        return await _context.Tasks.CountAsync(t => !t.Completed);
    }

    public async Task<int> CountUrgentActiveAsync()
    {
        return await _context.Tasks.CountAsync(t => 
            t.Priority == Priority.Urgent && !t.Completed);
    }
}
