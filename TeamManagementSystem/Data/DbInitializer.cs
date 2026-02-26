using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using TeamManagementSystem.Web.Data.Seed;
using TeamManagementSystem.Web.Models.Identity;

namespace TeamManagementSystem.Web.Data;

public static class DbInitializer
{
    public static async Task InitializeAsync(IHost app)
    {
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

        await db.Database.MigrateAsync();
        await SeedRolesAsync(roleManager);
        await SeedData.SeedAsync(db, userManager);
    }

    private static async Task SeedRolesAsync(RoleManager<ApplicationRole> roleManager)
    {
        string[] roleNames = { "Admin", "TeamLead", "Member", "Client" };
        foreach (var name in roleNames)
        {
            if (await roleManager.RoleExistsAsync(name)) continue;
            await roleManager.CreateAsync(new ApplicationRole(name));
        }
    }
}
