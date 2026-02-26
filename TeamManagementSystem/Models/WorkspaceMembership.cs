using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Models;

public enum WorkspaceRole { Admin = 1, Lead = 2, Member = 3, Client = 4 }

public class WorkspaceMembership
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    public DateTime JoinedAtUtc { get; set; } = DateTime.UtcNow;
}