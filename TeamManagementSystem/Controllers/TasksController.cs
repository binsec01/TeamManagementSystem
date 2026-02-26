using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Tasks;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class TasksController : Controller
{
    private readonly AppDbContext _db;
    private readonly ITaskQueryService _taskQuery;
    private readonly IAuthorizationServiceEx _authEx;
    private readonly INotificationService _notifications;
    private readonly IActivityService _activity;
    private readonly IFileStorageService _fileStorage;

    public TasksController(
        AppDbContext db,
        ITaskQueryService taskQuery,
        IAuthorizationServiceEx authEx,
        INotificationService notifications,
        IActivityService activity,
        IFileStorageService fileStorage)
    {
        _db = db;
        _taskQuery = taskQuery;
        _authEx = authEx;
        _notifications = notifications;
        _activity = activity;
        _fileStorage = fileStorage;
    }

    public async Task<IActionResult> List(int projectId, string? status, string? assigneeId, string? search, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, projectId)) return Forbid();

        var vm = await _taskQuery.GetTaskListAsync(projectId, status, assigneeId, search);
        ViewData["ProjectId"] = projectId;
        ViewData["CanManage"] = await _authEx.CanManageProjectAsync(user.Id, projectId);
        var project = await _db.Projects.Include(p => p.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.User).FirstAsync(p => p.Id == projectId, cancellationToken);
        ViewData["TeamMembers"] = project.Team.Members.Select(m => m.User).ToList();
        return View(vm);
    }

    public async Task<IActionResult> Board(int projectId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, projectId)) return Forbid();

        var vm = await _taskQuery.GetTaskBoardAsync(projectId);
        ViewData["ProjectId"] = projectId;
        ViewData["CanManage"] = await _authEx.CanManageProjectAsync(user.Id, projectId);
        return View(vm);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int projectId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanManageProjectAsync(user.Id, projectId)) return Forbid();

        var project = await _db.Projects.Include(p => p.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.User).FirstAsync(p => p.Id == projectId, cancellationToken);
        ViewData["TeamMembers"] = project.Team.Members.Select(m => m.User).ToList();
        return View(new TaskCreateVm { ProjectId = projectId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(TaskCreateVm model, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanManageProjectAsync(user.Id, model.ProjectId)) return Forbid();

        if (!ModelState.IsValid)
        {
            var project = await _db.Projects.Include(p => p.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.User).FirstAsync(p => p.Id == model.ProjectId, cancellationToken);
            ViewData["TeamMembers"] = project.Team.Members.Select(m => m.User).ToList();
            return View(model);
        }

        var task = new TaskItem
        {
            ProjectId = model.ProjectId,
            Title = model.Title,
            Description = model.Description,
            Priority = model.Priority,
            DueDateUtc = model.DueDateUtc,
            AssigneeId = model.AssigneeId,
            CreatedById = user.Id
        };
        _db.Tasks.Add(task);
        await _db.SaveChangesAsync(cancellationToken);

        if (!string.IsNullOrWhiteSpace(model.TagNames))
        {
            foreach (var name in model.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
                if (tag == null) { tag = new Tag { Name = name }; _db.Tags.Add(tag); await _db.SaveChangesAsync(cancellationToken); }
                _db.Set<TaskTag>().Add(new TaskTag { TaskItemId = task.Id, TagId = tag.Id });
            }
            await _db.SaveChangesAsync(cancellationToken);
        }

        await _activity.LogAsync(user.Id, "TaskItem", task.Id.ToString(), "Created");
        if (!string.IsNullOrEmpty(model.AssigneeId) && model.AssigneeId != user.Id)
            await _notifications.NotifyAssignmentAsync(model.AssigneeId, task.Id, task.Title, user.Id);

        return RedirectToAction(nameof(List), new { projectId = model.ProjectId });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks
            .Include(t => t.Project).ThenInclude(p => p!.Team)
            .Include(t => t.Assignee)
            .Include(t => t.Comments).ThenInclude(c => c.Author)
            .Include(t => t.Attachments)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, task.ProjectId)) return Forbid();

        ViewData["CanManage"] = await _authEx.CanManageProjectAsync(user.Id, task.ProjectId);
        ViewData["TeamMembers"] = await _db.TeamMemberships.Include(m => m.User).Where(m => m.TeamId == task.Project.TeamId).Select(m => m.User).ToListAsync(cancellationToken);
        return View(task);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.Include(t => t.TaskTags).ThenInclude(tt => tt.Tag).FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (task == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanManageProjectAsync(user.Id, task.ProjectId)) return Forbid();

        var vm = new TaskEditVm
        {
            Id = task.Id,
            ProjectId = task.ProjectId,
            Title = task.Title,
            Description = task.Description,
            Status = task.Status,
            Priority = task.Priority,
            DueDateUtc = task.DueDateUtc,
            AssigneeId = task.AssigneeId,
            TagNames = string.Join(", ", task.TaskTags.Select(tt => tt.Tag.Name))
        };
        var project = await _db.Projects.Include(p => p.Team).ThenInclude(t => t!.Members).ThenInclude(m => m.User).FirstAsync(p => p.Id == task.ProjectId, cancellationToken);
        ViewData["TeamMembers"] = project.Team.Members.Select(m => m.User).ToList();
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(TaskEditVm model, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanManageProjectAsync(user.Id, model.ProjectId)) return Forbid();

        var task = await _db.Tasks.Include(t => t.TaskTags).FirstOrDefaultAsync(t => t.Id == model.Id, cancellationToken);
        if (task == null) return NotFound();

        var oldStatus = task.Status;
        task.Title = model.Title;
        task.Description = model.Description;
        task.Status = model.Status;
        task.Priority = model.Priority;
        task.DueDateUtc = model.DueDateUtc;
        var previousAssignee = task.AssigneeId;
        task.AssigneeId = model.AssigneeId;

        _db.TaskTags.RemoveRange(task.TaskTags);
        if (!string.IsNullOrWhiteSpace(model.TagNames))
        {
            foreach (var name in model.TagNames.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                var tag = await _db.Tags.FirstOrDefaultAsync(t => t.Name == name, cancellationToken);
                if (tag == null) { tag = new Tag { Name = name }; _db.Tags.Add(tag); await _db.SaveChangesAsync(cancellationToken); }
                _db.Set<TaskTag>().Add(new TaskTag { TaskItemId = task.Id, TagId = tag.Id });
            }
        }
        await _db.SaveChangesAsync(cancellationToken);

        await _activity.LogAsync(user.Id, "TaskItem", task.Id.ToString(), "Updated");
        if (oldStatus != task.Status && !string.IsNullOrEmpty(task.AssigneeId))
            await _notifications.NotifyStatusChangeAsync(task.AssigneeId, task.Id, task.Title, oldStatus.ToString(), task.Status.ToString());
        if (!string.IsNullOrEmpty(model.AssigneeId) && model.AssigneeId != previousAssignee)
            await _notifications.NotifyAssignmentAsync(model.AssigneeId, task.Id, task.Title, user.Id);

        return RedirectToAction(nameof(Details), new { id = task.Id });
    }
}
