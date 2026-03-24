using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using TaskManager.Web.Controllers;
using TaskManager.Web.Models;
using Xunit;

namespace TaskManager.Web.Tests;

public class HomeControllerTests
{
    [Fact]
    public void Index_ReturnsView()
    {
        var loggerMock = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(loggerMock.Object);
        var result = controller.Index();
        Assert.IsType<ViewResult>(result);
    }

    [Fact]
    public void Privacy_ReturnsView()
    {
        var loggerMock = new Mock<ILogger<HomeController>>();
        var controller = new HomeController(loggerMock.Object);
        var result = controller.Privacy();
        Assert.IsType<ViewResult>(result);
    }
}
