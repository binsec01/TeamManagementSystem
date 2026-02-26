using System.ComponentModel.DataAnnotations;
using TeamManagementSystem.Web.Models;

namespace TeamManagementSystem.Web.ViewModels.Tasks;

public class TaskEditVm
{
    public int Id { get; set; }
    public int ProjectId { get; set; }

    [Required]
    [StringLength(300)]
    [Display(Name = "Title")]
    public string Title { get; set; } = default!;

    [StringLength(4000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Status")]
    public TeamManagementSystem.Web.Models.TaskStatus Status { get; set; }

    [Display(Name = "Priority")]
    public TaskPriority Priority { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDateUtc { get; set; }

    [Display(Name = "Assignee")]
    public string? AssigneeId { get; set; }

    [Display(Name = "Tags (comma-separated)")]
    public string? TagNames { get; set; }
}
