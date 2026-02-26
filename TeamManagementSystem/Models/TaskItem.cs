using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Models;

public enum TaskStatus { Backlog = 1, Todo = 2, InProgress = 3, Review = 4, Done = 5 }
public enum TaskPriority { Low = 1, Medium = 2, High = 3, Critical = 4 }

public class TaskItem
{
    public int Id { get; set; }
    public int ProjectId { get; set; }
    public Project Project { get; set; } = default!;

    public string Title { get; set; } = default!;
    public string? Description { get; set; }

    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public TaskPriority Priority { get; set; } = TaskPriority.Medium;

    public DateTime? DueDateUtc { get; set; }

    public string? AssigneeId { get; set; }
    public ApplicationUser? Assignee { get; set; }

    public string CreatedById { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<TaskComment> Comments { get; set; } = new List<TaskComment>();
    public ICollection<TaskAttachment> Attachments { get; set; } = new List<TaskAttachment>();
    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}