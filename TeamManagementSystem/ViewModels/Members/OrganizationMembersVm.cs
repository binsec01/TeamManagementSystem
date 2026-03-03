using System.ComponentModel.DataAnnotations;
using TeamManagementSystem.Web.Models;

namespace TeamManagementSystem.Web.ViewModels.Members;

public class OrganizationMembersVm
{
    public int OrganizationId { get; set; }
    public string OrganizationName { get; set; } = default!;
    public WorkspaceRole CurrentUserRole { get; set; }

    public List<OrganizationMemberRowVm> Members { get; set; } = new();
    public List<OrganizationInviteRowVm> PendingInvites { get; set; } = new();
}

public class OrganizationMemberRowVm
{
    public int MembershipId { get; set; }
    public string UserId { get; set; } = default!;
    public string DisplayName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public WorkspaceRole Role { get; set; }
    public DateTime JoinedAtUtc { get; set; }
}

public class OrganizationInviteRowVm
{
    public int Id { get; set; }
    public string? Email { get; set; }
    public WorkspaceRole Role { get; set; }
    public string Code { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; }
    public DateTime? ExpiresAtUtc { get; set; }
}

public class CreateInviteVm
{
    [Required]
    public int OrganizationId { get; set; }

    [Required]
    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    [EmailAddress]
    public string? Email { get; set; }
}

