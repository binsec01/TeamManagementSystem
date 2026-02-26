namespace TeamManagementSystem.Web.Models;

public class Organization
{
    public int Id { get; set; }
    public string Name { get; set; } = default!;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<Team> Teams { get; set; } = new List<Team>();
    public ICollection<WorkspaceMembership> WorkspaceMemberships { get; set; } = new List<WorkspaceMembership>();
}