using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class CommentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;
    private readonly INotificationService _notifications;
    private readonly IActivityService _activity;

    public CommentsController(AppDbContext db, IAuthorizationServiceEx authEx, INotificationService notifications, IActivityService activity)
    {
        _db = db;
        _authEx = authEx;
        _notifications = notifications;
        _activity = activity;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int taskId, string body, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.Include(t => t.Assignee).FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        if (task == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, task.ProjectId)) return Forbid();

        _db.TaskComments.Add(new TaskComment
        {
            TaskItemId = taskId,
            AuthorId = user.Id,
            Body = body ?? ""
        });
        await _db.SaveChangesAsync(cancellationToken);
        await _activity.LogAsync(user.Id, "TaskComment", taskId.ToString(), "Created");
        if (task.AssigneeId != null && task.AssigneeId != user.Id)
            await _notifications.NotifyCommentAsync(task.AssigneeId, taskId, task.Title, user.Id);
        return RedirectToAction("Details", "Tasks", new { id = taskId });
    }
}
