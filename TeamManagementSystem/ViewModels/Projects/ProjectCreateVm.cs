using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Projects;

public class ProjectCreateVm
{
    public int TeamId { get; set; }

    [Required]
    [StringLength(200)]
    [Display(Name = "Project Name")]
    public string Name { get; set; } = default!;

    [StringLength(2000)]
    [Display(Name = "Description")]
    public string? Description { get; set; }

    [Display(Name = "Start Date")]
    [DataType(DataType.Date)]
    public DateTime? StartDateUtc { get; set; }

    [Display(Name = "Due Date")]
    [DataType(DataType.Date)]
    public DateTime? DueDateUtc { get; set; }
}
