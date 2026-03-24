using TaskManager.Web.Models;

namespace TaskManager.Web.Repositories;

/// <summary>
/// Interface para o repositório de tarefas
/// </summary>
public interface ITaskRepository
{
    Task<IEnumerable<TaskItem>> GetAllAsync(int page = 1, int pageSize = 2, bool asc = false);
    Task<TaskItem?> GetByIdAsync(long id);
    Task<TaskItem> CreateAsync(TaskItem task);
    Task<TaskItem?> UpdateAsync(TaskItem task);
    Task<bool> DeleteAsync(long id);
    Task<int> CountAllAsync();
    Task<int> CountCompletedAsync();
    Task<int> CountPendingAsync();
    Task<int> CountUrgentActiveAsync();
}
