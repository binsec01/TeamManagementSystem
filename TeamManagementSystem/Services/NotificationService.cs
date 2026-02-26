using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Services;

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task NotifyAssignmentAsync(string recipientId, int taskId, string taskTitle, string assignedByUserId)
    {
        _db.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            Type = NotificationType.Assignment,
            Message = $"You were assigned to task: {taskTitle}",
            Url = $"/Tasks/Details/{taskId}",
            IsRead = false
        });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyMentionAsync(string recipientId, int taskId, string taskTitle, string mentionedByUserId)
    {
        _db.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            Type = NotificationType.Mention,
            Message = $"You were mentioned in task: {taskTitle}",
            Url = $"/Tasks/Details/{taskId}",
            IsRead = false
        });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyStatusChangeAsync(string recipientId, int taskId, string taskTitle, string oldStatus, string newStatus)
    {
        _db.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            Type = NotificationType.StatusChange,
            Message = $"Task '{taskTitle}' changed from {oldStatus} to {newStatus}",
            Url = $"/Tasks/Details/{taskId}",
            IsRead = false
        });
        await _db.SaveChangesAsync();
    }

    public async Task NotifyCommentAsync(string recipientId, int taskId, string taskTitle, string commentAuthorId)
    {
        _db.Notifications.Add(new Notification
        {
            RecipientId = recipientId,
            Type = NotificationType.Comment,
            Message = $"New comment on task: {taskTitle}",
            Url = $"/Tasks/Details/{taskId}",
            IsRead = false
        });
        await _db.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(int notificationId, string userId)
    {
        var n = await _db.Notifications.FirstOrDefaultAsync(x => x.Id == notificationId && x.RecipientId == userId);
        if (n != null) { n.IsRead = true; await _db.SaveChangesAsync(); }
    }

    public async Task MarkAllAsReadAsync(string userId)
    {
        var list = await _db.Notifications.Where(x => x.RecipientId == userId && !x.IsRead).ToListAsync();
        foreach (var n in list) n.IsRead = true;
        await _db.SaveChangesAsync();
    }
}
