using Microsoft.AspNetCore.Identity;

namespace TeamManagementSystem.Web.Models.Identity;

public class ApplicationUser : IdentityUser
{
    public string? FullName { get; set; }
}