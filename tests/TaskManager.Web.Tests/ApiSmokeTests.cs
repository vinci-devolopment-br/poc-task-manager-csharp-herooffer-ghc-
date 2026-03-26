using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TaskManager.Web.Data;
using Xunit;

namespace TaskManager.Web.Tests;

public class ApiSmokeTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public ApiSmokeTests(WebApplicationFactory<Program> factory)
    {
        var customFactory = factory.WithWebHostBuilder(builder =>
        {
            // Define ambiente "Testing" para que IsDevelopment() == false
            // e o EnsureCreated() do Program.cs não seja executado
            builder.UseEnvironment("Testing");

            builder.ConfigureServices(services =>
            {
                // Remove TODOS os serviços relacionados ao TaskManagerDbContext,
                // incluindo IDbContextOptionsConfiguration<T> registrado internamente pelo EF Core.
                // Sem isso, SqlServer e InMemory ficam registrados juntos e causam conflito.
                var dbContextType = typeof(TaskManagerDbContext);
                var toRemove = services.Where(d =>
                    d.ServiceType == dbContextType ||
                    d.ServiceType == typeof(DbContextOptions<TaskManagerDbContext>) ||
                    (d.ServiceType.IsGenericType &&
                     d.ServiceType.GenericTypeArguments.Contains(dbContextType))
                ).ToList();

                foreach (var d in toRemove)
                    services.Remove(d);

                // Registra InMemory — funciona em Linux, Mac e Windows
                services.AddDbContext<TaskManagerDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_ApiSmoke"));
            });
        });

        _client = customFactory.CreateClient();
    }

    [Fact]
    public async Task Get_Home_Index_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/");
        response.EnsureSuccessStatusCode();
    }

    [Fact]
    public async Task Get_Tasks_Index_ReturnsSuccess()
    {
        var response = await _client.GetAsync("/Tasks");
        Assert.True(response.IsSuccessStatusCode);
    }
}
