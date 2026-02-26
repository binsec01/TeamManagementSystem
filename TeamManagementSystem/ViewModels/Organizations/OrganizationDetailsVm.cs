namespace TeamManagementSystem.Web.ViewModels.Organizations;

public class OrganizationDetailsVm
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public List<TeamSummaryVm> Teams { get; set; } = new();
    public string? CurrentUserRole { get; set; }
}

public class TeamSummaryVm
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public int ProjectCount { get; set; }
    public int MemberCount { get; set; }
}
