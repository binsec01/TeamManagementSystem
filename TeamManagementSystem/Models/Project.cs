namespace TeamManagementSystem.Web.Models;

public class Project
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = default!;

    public string Name { get; set; } = default!;
    public string? Description { get; set; }

    public DateTime? StartDateUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }

    public ICollection<TaskItem> Tasks { get; set; } = new List<TaskItem>();
}