using Microsoft.AspNetCore.Mvc;
using TaskManager.Web.Models;
using TaskManager.Web.Services;

namespace TaskManager.Web.Controllers;

public class TasksController : Controller
{
    // GET: /Tasks/TaskListPartial
    public async Task<IActionResult> TaskListPartial(int page = 1, string order = "desc")
    {
        const int pageSize = 2;
        bool asc = order == "asc";
        var tasks = await _taskService.GetAllTasksAsync(page, pageSize, asc);
        var totalTasks = await _taskService.GetStatisticsAsync();
        ViewBag.CurrentPage = page;
        ViewBag.TotalPages = (int)Math.Ceiling((double)totalTasks.Total / pageSize);
        ViewBag.Order = order;
        return PartialView("_TaskListPartial", tasks);
    }
    private readonly ITaskService _taskService;
    private readonly ILogger<TasksController> _logger;

    public TasksController(ITaskService taskService, ILogger<TasksController> logger)
    {
        _taskService = taskService;
        _logger = logger;
    }

    // GET: /Tasks
    public async Task<IActionResult> Index(int page = 1, string order = "desc")
    {
        try
        {
            const int pageSize = 2;
            bool asc = order == "asc";
            var tasks = await _taskService.GetAllTasksAsync(page, pageSize, asc);
            var totalTasks = await _taskService.GetStatisticsAsync();
            ViewBag.CurrentPage = page;
            ViewBag.TotalPages = (int)Math.Ceiling((double)totalTasks.Total / pageSize);
            ViewBag.Order = order;
            return View(tasks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar lista de tarefas");
            TempData["Error"] = "Erro ao carregar tarefas";
            return View(new List<TaskItem>());
        }
    }

    // GET: /Tasks/Create
    public IActionResult Create()
    {
        return View();
    }

    // POST: /Tasks/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskItem task)
    {
        if (!ModelState.IsValid)
        {
            return View(task);
        }

        try
        {
            // Por enquanto, usar ID de usuário padrão (sem autenticação)
            task.UserId = "default-user";
            task.CreatedAt = DateTime.UtcNow;
            task.UpdatedAt = DateTime.UtcNow;
            await _taskService.CreateTaskAsync(task);
            TempData["Success"] = "Tarefa criada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao criar tarefa");
            ModelState.AddModelError("", "Erro ao criar tarefa");
            return View(task);
        }
    }

    // GET: /Tasks/Edit/5
    public async Task<IActionResult> Edit(long id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar tarefa {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Tasks/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(long id, TaskItem task)
    {
        if (id != task.Id)
        {
            return BadRequest();
        }

        if (!ModelState.IsValid)
        {
            return View(task);
        }

        try
        {
            var updated = await _taskService.UpdateTaskAsync(task);
            if (updated == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            TempData["Success"] = "Tarefa atualizada com sucesso!";
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao atualizar tarefa {TaskId}", id);
            ModelState.AddModelError("", "Erro ao atualizar tarefa");
            return View(task);
        }
    }

    // GET: /Tasks/Delete/5
    public async Task<IActionResult> Delete(long id)
    {
        try
        {
            var task = await _taskService.GetTaskByIdAsync(id);
            if (task == null)
            {
                TempData["Error"] = "Tarefa não encontrada";
                return RedirectToAction(nameof(Index));
            }
            return View(task);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao carregar tarefa {TaskId}", id);
            TempData["Error"] = "Erro ao carregar tarefa";
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Tasks/Delete/5
    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            var deleted = await _taskService.DeleteTaskAsync(id);
            if (!deleted)
            {
                TempData["Error"] = "Tarefa não encontrada";
            }
            else
            {
                TempData["Success"] = "Tarefa excluída com sucesso!";
            }
            return RedirectToAction(nameof(Index));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao excluir tarefa {TaskId}", id);
            TempData["Error"] = "Erro ao excluir tarefa";
            return RedirectToAction(nameof(Index));
        }
    }
}
