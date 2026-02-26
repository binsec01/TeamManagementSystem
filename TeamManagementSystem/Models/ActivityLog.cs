namespace TeamManagementSystem.Web.Models;

public class ActivityLog
{
    public int Id { get; set; }
    public string ActorUserId { get; set; } = default!;

    public string EntityType { get; set; } = default!;   // e.g. "TaskItem"
    public string EntityId { get; set; } = default!;     // store as string for flexibility
    public string Action { get; set; } = default!;       // e.g. "Created", "Updated", "StatusChanged"
    public string? DetailsJson { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}