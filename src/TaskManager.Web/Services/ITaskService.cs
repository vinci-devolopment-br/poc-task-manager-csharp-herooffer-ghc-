using TaskManager.Web.Models;

namespace TaskManager.Web.Services;

/// <summary>
/// Interface para o serviço de tarefas
/// </summary>
public interface ITaskService
{
    Task<IEnumerable<TaskItem>> GetAllTasksAsync(int page = 1, int pageSize = 2, bool asc = false);
    Task<TaskItem?> GetTaskByIdAsync(long id);
    Task<TaskItem> CreateTaskAsync(TaskItem task);
    Task<TaskItem?> UpdateTaskAsync(TaskItem task);
    Task<bool> DeleteTaskAsync(long id);
    Task<TaskStatistics> GetStatisticsAsync();
}

/// <summary>
/// DTO para estatísticas de tarefas
/// </summary>
public class TaskStatistics
{
    public int Total { get; set; }
    public int Completed { get; set; }
    public int Pending { get; set; }
    public int UrgentActive { get; set; }
}
