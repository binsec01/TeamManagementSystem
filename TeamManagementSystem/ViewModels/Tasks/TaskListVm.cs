using TeamManagementSystem.Web.Models;

namespace TeamManagementSystem.Web.ViewModels.Tasks;

public class TaskListVm
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = default!;
    public List<TaskRowVm> Tasks { get; set; } = new();
    public string? FilterStatus { get; set; }
    public string? FilterAssigneeId { get; set; }
    public string? Search { get; set; }
}

public class TaskRowVm
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public TeamManagementSystem.Web.Models.TaskStatus Status { get; set; }
    public TaskPriority Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public string? AssigneeName { get; set; }
    public List<string> TagNames { get; set; } = new();
}
