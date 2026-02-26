namespace TeamManagementSystem.Web.Models;

public class TaskAttachment
{
    public int Id { get; set; }
    public int TaskItemId { get; set; }
    public TaskItem TaskItem { get; set; } = default!;

    public string OriginalFileName { get; set; } = default!;
    public string StoredFileName { get; set; } = default!;
    public string ContentType { get; set; } = default!;
    public long SizeBytes { get; set; }

    public string UploadedById { get; set; } = default!;
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}