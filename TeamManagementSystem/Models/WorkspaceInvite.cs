using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Models;

public class WorkspaceInvite
{
    public int Id { get; set; }

    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = default!;

    public string Code { get; set; } = Guid.NewGuid().ToString("N");

    public string? Email { get; set; }

    public WorkspaceRole Role { get; set; } = WorkspaceRole.Member;

    public string CreatedById { get; set; } = default!;
    public ApplicationUser CreatedBy { get; set; } = default!;

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? ExpiresAtUtc { get; set; }

    public DateTime? AcceptedAtUtc { get; set; }
    public string? AcceptedByUserId { get; set; }
    public ApplicationUser? AcceptedByUser { get; set; }

    public bool IsRevoked { get; set; }
}

