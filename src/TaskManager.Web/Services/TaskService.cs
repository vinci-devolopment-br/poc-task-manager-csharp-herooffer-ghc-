using TaskManager.Web.Models;
using TaskManager.Web.Repositories;

namespace TaskManager.Web.Services;

/// <summary>
/// Implementação do serviço de tarefas com lógica de negócio
/// </summary>
public class TaskService : ITaskService
{
    private readonly ITaskRepository _repository;

    public TaskService(ITaskRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<TaskItem>> GetAllTasksAsync(int page = 1, int pageSize = 2, bool asc = false)
    {
        return await _repository.GetAllAsync(page, pageSize, asc);
    }

    public async Task<TaskItem?> GetTaskByIdAsync(long id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<TaskItem> CreateTaskAsync(TaskItem task)
    {
        // Validações de negócio podem ser adicionadas aqui
        return await _repository.CreateAsync(task);
    }

    public async Task<TaskItem?> UpdateTaskAsync(TaskItem task)
    {
        // Validações de negócio podem ser adicionadas aqui
        return await _repository.UpdateAsync(task);
    }

    public async Task<bool> DeleteTaskAsync(long id)
    {
        return await _repository.DeleteAsync(id);
    }

    public async Task<TaskStatistics> GetStatisticsAsync()
    {
        var total = await _repository.CountAllAsync();
        var completed = await _repository.CountCompletedAsync();
        var pending = await _repository.CountPendingAsync();
        var urgentActive = await _repository.CountUrgentActiveAsync();

        return new TaskStatistics
        {
            Total = total,
            Completed = completed,
            Pending = pending,
            UrgentActive = urgentActive
        };
    }
}
