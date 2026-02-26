namespace TeamManagementSystem.Web.Services.Interfaces;

public interface INotificationService
{
    Task NotifyAssignmentAsync(string recipientId, int taskId, string taskTitle, string assignedByUserId);
    Task NotifyMentionAsync(string recipientId, int taskId, string taskTitle, string mentionedByUserId);
    Task NotifyStatusChangeAsync(string recipientId, int taskId, string taskTitle, string oldStatus, string newStatus);
    Task NotifyCommentAsync(string recipientId, int taskId, string taskTitle, string commentAuthorId);
    Task MarkAsReadAsync(int notificationId, string userId);
    Task MarkAllAsReadAsync(string userId);
}
