namespace TeamManagementSystem.Web.ViewModels.Reports;

public class TeamWorkloadReportVm
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;
    public List<MemberWorkloadRowVm> Members { get; set; } = new();
}

public class MemberWorkloadRowVm
{
    public string UserId { get; set; } = default!;
    public string FullName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public int TotalTasks { get; set; }
    public int OpenTasks { get; set; }
    public int InProgressTasks { get; set; }
    public int CompletedTasks { get; set; }
    public int OverdueTasks { get; set; }
}
