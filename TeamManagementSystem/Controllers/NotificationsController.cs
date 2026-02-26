using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class NotificationsController : Controller
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notificationService;

    public NotificationsController(AppDbContext db, INotificationService notificationService)
    {
        _db = db;
        _notificationService = notificationService;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");

        var list = await _db.Notifications
            .Where(n => n.RecipientId == user.Id)
            .OrderByDescending(n => n.CreatedAtUtc)
            .Take(100)
            .ToListAsync(cancellationToken);
        return View(list);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkRead(int id, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        await _notificationService.MarkAsReadAsync(id, user.Id);
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> MarkAllRead(CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        await _notificationService.MarkAllAsReadAsync(user.Id);
        return RedirectToAction(nameof(Index));
    }
}
