using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class AttachmentsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;
    private readonly IFileStorageService _fileStorage;
    private readonly IActivityService _activity;

    public AttachmentsController(AppDbContext db, IAuthorizationServiceEx authEx, IFileStorageService fileStorage, IActivityService activity)
    {
        _db = db;
        _authEx = authEx;
        _fileStorage = fileStorage;
        _activity = activity;
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Upload(int taskId, IFormFile file, CancellationToken cancellationToken)
    {
        var task = await _db.Tasks.FirstOrDefaultAsync(t => t.Id == taskId, cancellationToken);
        if (task == null) return NotFound();
        if (file == null || file.Length == 0) return RedirectToAction("Details", "Tasks", new { id = taskId });

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, task.ProjectId)) return Forbid();

        await using var stream = file.OpenReadStream();
        var (stored, contentType, size) = await _fileStorage.SaveAsync(stream, file.FileName);
        _db.TaskAttachments.Add(new TaskAttachment
        {
            TaskItemId = taskId,
            OriginalFileName = file.FileName,
            StoredFileName = stored,
            ContentType = contentType,
            SizeBytes = size,
            UploadedById = user.Id
        });
        await _db.SaveChangesAsync(cancellationToken);
        await _activity.LogAsync(user.Id, "TaskAttachment", taskId.ToString(), "Uploaded");
        return RedirectToAction("Details", "Tasks", new { id = taskId });
    }

    [HttpGet]
    public async Task<IActionResult> Download(int id, CancellationToken cancellationToken)
    {
        var att = await _db.TaskAttachments.Include(a => a.TaskItem).FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        if (att == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, att.TaskItem.ProjectId)) return Forbid();

        var stream = await _fileStorage.GetAsync(att.StoredFileName);
        if (stream == null) return NotFound();
        return File(stream, att.ContentType, att.OriginalFileName);
    }
}
