namespace TeamManagementSystem.Web.ViewModels.Organizations;

public class OrganizationDetailsVm
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public string? CurrentUserRole { get; set; }
    public DateTime CreatedAtUtc { get; set; }
    public int MemberCount { get; set; }
    public int ProjectCount { get; set; }
    public int? FirstTeamId { get; set; }
}
