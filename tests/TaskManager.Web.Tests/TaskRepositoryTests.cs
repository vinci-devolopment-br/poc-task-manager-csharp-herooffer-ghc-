using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using TaskManager.Web.Data;
using TaskManager.Web.Models;
using TaskManager.Web.Repositories;
using Xunit;

namespace TaskManager.Web.Tests;

public class TaskRepositoryTests
{
    private TaskManagerDbContext GetDbContext(string dbName)
    {
        var options = new DbContextOptionsBuilder<TaskManagerDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
        return new TaskManagerDbContext(options);
    }

    [Fact]
    public async Task CreateAndGetTask_Works()
    {
        var db = GetDbContext("CreateAndGetTask");
        var repo = new TaskRepository(db);
        var task = new TaskItem { Title = "RepoTest", UserId = "u1" };
        await repo.CreateAsync(task);
        var all = await repo.GetAllAsync();
        Assert.Single(all);
    }

    [Fact]
    public async Task DeleteTask_RemovesTask()
    {
        var db = GetDbContext("DeleteTask");
        var repo = new TaskRepository(db);
        var task = new TaskItem { Title = "ToDelete", UserId = "u2" };
        var created = await repo.CreateAsync(task);
        var deleted = await repo.DeleteAsync(created.Id);
        Assert.True(deleted);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsTask_WhenExists()
    {
        var db = GetDbContext("GetByIdExists");
        var repo = new TaskRepository(db);
        var task = new TaskItem { Title = "FindMe", UserId = "u3" };
        var created = await repo.CreateAsync(task);
        var found = await repo.GetByIdAsync(created.Id);
        Assert.NotNull(found);
        Assert.Equal(created.Id, found.Id);
    }

    [Fact]
    public async Task GetByIdAsync_ReturnsNull_WhenNotExists()
    {
        var db = GetDbContext("GetByIdNotExists");
        var repo = new TaskRepository(db);
        var found = await repo.GetByIdAsync(999);
        Assert.Null(found);
    }

    [Fact]
    public async Task UpdateAsync_UpdatesTask_WhenExists()
    {
        var db = GetDbContext("UpdateExists");
        var repo = new TaskRepository(db);
        var task = new TaskItem { Title = "Old", UserId = "u4" };
        var created = await repo.CreateAsync(task);
        created.Title = "New";
        var updated = await repo.UpdateAsync(created);
        Assert.NotNull(updated);
        Assert.Equal("New", updated.Title);
    }

    [Fact]
    public async Task UpdateAsync_ReturnsNull_WhenNotExists()
    {
        var db = GetDbContext("UpdateNotExists");
        var repo = new TaskRepository(db);
        var task = new TaskItem { Id = 999, Title = "NotFound", UserId = "u5" };
        var updated = await repo.UpdateAsync(task);
        Assert.Null(updated);
    }

    [Fact]
    public async Task DeleteAsync_ReturnsFalse_WhenNotExists()
    {
        var db = GetDbContext("DeleteNotExists");
        var repo = new TaskRepository(db);
        var deleted = await repo.DeleteAsync(999);
        Assert.False(deleted);
    }

    [Fact]
    public async Task CountMethods_ReturnsCorrectCounts()
    {
        var db = GetDbContext("CountMethods");
        var repo = new TaskRepository(db);
        await repo.CreateAsync(new TaskItem { Title = "T1", Completed = false, Priority = Priority.Medium });
        await repo.CreateAsync(new TaskItem { Title = "T2", Completed = true, Priority = Priority.Urgent });
        await repo.CreateAsync(new TaskItem { Title = "T3", Completed = false, Priority = Priority.Urgent });
        Assert.Equal(3, await repo.CountAllAsync());
        Assert.Equal(1, await repo.CountCompletedAsync());
        Assert.Equal(2, await repo.CountPendingAsync());
        Assert.Equal(1, await repo.CountUrgentActiveAsync());
    }
}
