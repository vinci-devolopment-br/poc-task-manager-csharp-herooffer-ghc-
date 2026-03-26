using System.Collections.Generic;
using System.Threading.Tasks;
using Moq;
using TaskManager.Web.Models;
using TaskManager.Web.Repositories;
using TaskManager.Web.Services;
using Xunit;

namespace TaskManager.Web.Tests;

public class TaskServiceTests
{
    private readonly Mock<ITaskRepository> _repoMock;
    private readonly TaskService _service;

    public TaskServiceTests()
    {
        _repoMock = new Mock<ITaskRepository>();
        _service = new TaskService(_repoMock.Object);
    }

    [Fact]
    public async Task GetAllTasksAsync_ReturnsTasks()
    {
        var tasks = new List<TaskItem> { new TaskItem { Id = 1, Title = "Test" } };
        _repoMock.Setup(r => r.GetAllAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>())).ReturnsAsync(tasks);
        var result = await _service.GetAllTasksAsync();
        Assert.Single(result);
    }

    [Fact]
    public async Task CreateTaskAsync_CallsRepository()
    {
        var task = new TaskItem { Id = 1, Title = "Test" };
        _repoMock.Setup(r => r.CreateAsync(task)).ReturnsAsync(task);
        var result = await _service.CreateTaskAsync(task);
        Assert.Equal(task.Id, result.Id);
    }

    [Fact]
    public async Task DeleteTaskAsync_ReturnsTrue()
    {
        _repoMock.Setup(r => r.DeleteAsync(1)).ReturnsAsync(true);
        var result = await _service.DeleteTaskAsync(1);
        Assert.True(result);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsTask_WhenExists()
    {
        var task = new TaskItem { Id = 2, Title = "FindMe" };
        _repoMock.Setup(r => r.GetByIdAsync(2)).ReturnsAsync(task);
        var result = await _service.GetTaskByIdAsync(2);
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public async Task GetTaskByIdAsync_ReturnsNull_WhenNotExists()
    {
        _repoMock.Setup(r => r.GetByIdAsync(99)).ReturnsAsync((TaskItem?)null);
        var result = await _service.GetTaskByIdAsync(99);
        Assert.Null(result);
    }

    [Fact]
    public async Task UpdateTaskAsync_ReturnsUpdatedTask_WhenExists()
    {
        var task = new TaskItem { Id = 3, Title = "UpdateMe" };
        _repoMock.Setup(r => r.UpdateAsync(task)).ReturnsAsync(task);
        var result = await _service.UpdateTaskAsync(task);
        Assert.NotNull(result);
        Assert.Equal(3, result.Id);
    }

    [Fact]
    public async Task UpdateTaskAsync_ReturnsNull_WhenNotExists()
    {
        var task = new TaskItem { Id = 99, Title = "NotFound" };
        _repoMock.Setup(r => r.UpdateAsync(task)).ReturnsAsync((TaskItem?)null);
        var result = await _service.UpdateTaskAsync(task);
        Assert.Null(result);
    }

    [Fact]
    public async Task GetStatisticsAsync_ReturnsCorrectStats()
    {
        _repoMock.Setup(r => r.CountAllAsync()).ReturnsAsync(10);
        _repoMock.Setup(r => r.CountCompletedAsync()).ReturnsAsync(4);
        _repoMock.Setup(r => r.CountPendingAsync()).ReturnsAsync(6);
        _repoMock.Setup(r => r.CountUrgentActiveAsync()).ReturnsAsync(2);
        var stats = await _service.GetStatisticsAsync();
        Assert.Equal(10, stats.Total);
        Assert.Equal(4, stats.Completed);
        Assert.Equal(6, stats.Pending);
        Assert.Equal(2, stats.UrgentActive);
    }
}
