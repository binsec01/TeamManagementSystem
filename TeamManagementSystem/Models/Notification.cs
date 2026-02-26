namespace TeamManagementSystem.Web.Models;

public enum NotificationType { Assignment=1, Mention=2, StatusChange=3, Comment=4 }

public class Notification
{
    public int Id { get; set; }

    public string RecipientId { get; set; } = default!;
    public NotificationType Type { get; set; }
    public string Message { get; set; } = default!;
    public string? Url { get; set; }

    public bool IsRead { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}