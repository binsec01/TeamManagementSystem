using Microsoft.AspNetCore.Identity;

namespace TeamManagementSystem.Web.Models.Identity;

public class ApplicationRole : IdentityRole
{
    public ApplicationRole() { }
    public ApplicationRole(string roleName) : base(roleName) { }
    public string? Description { get; set; }
}
