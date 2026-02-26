using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Models;

public class TeamMembership
{
    public int Id { get; set; }
    public int TeamId { get; set; }
    public Team Team { get; set; } = default!;

    public string UserId { get; set; } = default!;
    public ApplicationUser User { get; set; } = default!;
}