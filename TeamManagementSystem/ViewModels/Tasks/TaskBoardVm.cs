using TeamManagementSystem.Web.Models;
using TaskStatusEnum = TeamManagementSystem.Web.Models.TaskStatus;

namespace TeamManagementSystem.Web.ViewModels.Tasks;

public class TaskBoardVm
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = default!;
    public Dictionary<TaskStatusEnum, List<TaskCardVm>> Columns { get; set; } = new();
}

public class TaskCardVm
{
    public int Id { get; set; }
    public string Title { get; set; } = default!;
    public TaskPriority Priority { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public string? AssigneeName { get; set; }
    public string? AssigneeId { get; set; }
    public List<string> TagNames { get; set; } = new();
}
