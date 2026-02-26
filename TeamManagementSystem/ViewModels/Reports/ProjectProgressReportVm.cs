namespace TeamManagementSystem.Web.ViewModels.Reports;

public class ProjectProgressReportVm
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;
    public List<ProjectProgressRowVm> Projects { get; set; } = new();
}

public class ProjectProgressRowVm
{
    public int ProjectId { get; set; }
    public string ProjectName { get; set; } = default!;
    public string TeamName { get; set; } = default!;
    public int OpenTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
    public int TotalTasks { get; set; }
}
