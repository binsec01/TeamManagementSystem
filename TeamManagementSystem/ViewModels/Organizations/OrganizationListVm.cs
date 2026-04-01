using TeamManagementSystem.Web.Models;

namespace TeamManagementSystem.Web.ViewModels.Organizations;

public class OrganizationListVm
{
    public Organization Organization { get; set; } = null!;
    public WorkspaceRole Role { get; set; }
    public int? FirstTeamId { get; set; }
}
