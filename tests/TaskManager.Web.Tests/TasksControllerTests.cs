using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Logging;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using TaskManager.Web.Controllers;
using TaskManager.Web.Models;
using TaskManager.Web.Services;
using Xunit;
using Microsoft.AspNetCore.Http;

namespace TaskManager.Web.Tests;

public class TasksControllerTests
{
    private TasksController CreateControllerWithTempData(Mock<ITaskService>? serviceMock = null)
    {
        var loggerMock = new Mock<ILogger<TasksController>>();
        var controller = new TasksController(
            serviceMock?.Object ?? new Mock<ITaskService>().Object,
            loggerMock.Object
        );
        controller.TempData = new TempDataDictionary(new DefaultHttpContext(), Mock.Of<ITempDataProvider>());
        return controller;
    }

    [Fact]
    public async Task Index_ReturnsViewWithTasks()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.GetAllTasksAsync()).ReturnsAsync(new List<TaskItem> { new TaskItem { Id = 1, Title = "T1" } });
        var controller = CreateControllerWithTempData(serviceMock);
        var result = await controller.Index();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }

    [Fact]
    public void Create_Get_ReturnsView()
    {
        var controller = CreateControllerWithTempData();
        var result = controller.Create();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public async Task Index_WhenServiceThrows_ReturnsEmptyListAndSetsError()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.GetAllTasksAsync()).ThrowsAsync(new System.Exception("fail"));
        var controller = CreateControllerWithTempData(serviceMock);
        var result = await controller.Index();
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.IsAssignableFrom<List<TaskItem>>(viewResult.Model);
        Assert.True(controller.TempData.ContainsKey("Error"));
    }

    [Fact]
    public async Task Edit_Get_ReturnsBadRequest_WhenIdMismatch()
    {
        var controller = CreateControllerWithTempData();
        var task = new TaskItem { Id = 2, Title = "T" };
        var result = await controller.Edit(1, task);
        Assert.IsType<BadRequestResult>(result);
    }

    [Fact]
    public async Task Edit_Get_ReturnsView_WhenModelStateInvalid()
    {
        var controller = CreateControllerWithTempData();
        controller.ModelState.AddModelError("Title", "Required");
        var task = new TaskItem { Id = 1, Title = "" };
        var result = await controller.Edit(1, task);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(task, viewResult.Model);
    }

    [Fact]
    public async Task Edit_Get_Redirects_WhenUpdateReturnsNull()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.UpdateTaskAsync(It.IsAny<TaskItem>())).ReturnsAsync((TaskItem?)null);
        var controller = CreateControllerWithTempData(serviceMock);
        var task = new TaskItem { Id = 1, Title = "T" };
        var result = await controller.Edit(1, task);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Edit_Get_ReturnsView_WhenExceptionThrown()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.UpdateTaskAsync(It.IsAny<TaskItem>())).ThrowsAsync(new System.Exception("fail"));
        var controller = CreateControllerWithTempData(serviceMock);
        var task = new TaskItem { Id = 1, Title = "T" };
        var result = await controller.Edit(1, task);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.Equal(task, viewResult.Model);
    }

    [Fact]
    public async Task Delete_Get_Redirects_WhenTaskNotFound()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<long>())).ReturnsAsync((TaskItem?)null);
        var controller = CreateControllerWithTempData(serviceMock);
        var result = await controller.Delete(1);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }

    [Fact]
    public async Task Delete_Get_ReturnsView_WhenTaskFound()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<long>())).ReturnsAsync(new TaskItem { Id = 1, Title = "T" });
        var controller = CreateControllerWithTempData(serviceMock);
        var result = await controller.Delete(1);
        var viewResult = Assert.IsType<ViewResult>(result);
        Assert.NotNull(viewResult.Model);
    }
    
    [Fact]
    public async Task Delete_Get_Redirects_WhenExceptionThrown()
    {
        var serviceMock = new Mock<ITaskService>();
        serviceMock.Setup(s => s.GetTaskByIdAsync(It.IsAny<long>())).ThrowsAsync(new System.Exception("fail"));
        var controller = CreateControllerWithTempData(serviceMock);
        var result = await controller.Delete(1);
        var redirect = Assert.IsType<RedirectToActionResult>(result);
        Assert.Equal("Index", redirect.ActionName);
    }
}
