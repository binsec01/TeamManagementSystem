using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Data.Seed;

public static class SeedData
{
    public static async Task SeedAsync(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        if (await db.Organizations.AnyAsync()) return;

        var admin = new ApplicationUser
        {
            UserName = "admin@tms.local",
            Email = "admin@tms.local",
            FullName = "System Admin",
            EmailConfirmed = true
        };
        await userManager.CreateAsync(admin, "Admin@123");
        await userManager.AddToRoleAsync(admin, "Admin");

        var org = new Organization { Name = "Default Organization" };
        db.Organizations.Add(org);
        await db.SaveChangesAsync();

        db.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            OrganizationId = org.Id,
            UserId = admin.Id,
            Role = WorkspaceRole.Admin
        });

        var team = new Team { OrganizationId = org.Id, Name = "Default Team" };
        db.Teams.Add(team);
        await db.SaveChangesAsync();

        db.TeamMemberships.Add(new TeamMembership { TeamId = team.Id, UserId = admin.Id });
        await db.SaveChangesAsync();
    }
}
