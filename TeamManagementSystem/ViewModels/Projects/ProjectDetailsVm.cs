namespace TeamManagementSystem.Web.ViewModels.Projects;

public class ProjectDetailsVm
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public string TeamName { get; set; } = default!;
    public string Name { get; set; } = default!;
    public string? Description { get; set; }
    public DateTime? StartDateUtc { get; set; }
    public DateTime? DueDateUtc { get; set; }
    public int OpenTaskCount { get; set; }
    public int CompletedTaskCount { get; set; }
    public int OverdueTaskCount { get; set; }
}
