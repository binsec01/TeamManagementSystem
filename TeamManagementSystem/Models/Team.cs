namespace TeamManagementSystem.Web.Models;

public class Team
{
    public int Id { get; set; }
    public int OrganizationId { get; set; }
    public Organization Organization { get; set; } = default!;

    public string Name { get; set; } = default!;
    public ICollection<TeamMembership> Members { get; set; } = new List<TeamMembership>();
    public ICollection<Project> Projects { get; set; } = new List<Project>();
}