using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Services;

public class AuthorizationServiceEx : IAuthorizationServiceEx
{
    private readonly AppDbContext _db;

    public AuthorizationServiceEx(AppDbContext db)
    {
        _db = db;
    }

    public async Task<bool> CanManageOrganizationAsync(string userId, int organizationId)
    {
        var role = await GetUserRoleInOrganizationAsync(userId, organizationId);
        return role is WorkspaceRole.Admin;
    }

    public async Task<bool> CanManageTeamAsync(string userId, int teamId)
    {
        var team = await _db.Teams
            .Include(t => t.Organization)
            .ThenInclude(o => o!.WorkspaceMemberships)
            .FirstOrDefaultAsync(t => t.Id == teamId);
        if (team == null) return false;
        var role = await GetUserRoleInOrganizationAsync(userId, team.OrganizationId);
        return role is WorkspaceRole.Admin or WorkspaceRole.Lead;
    }

    public async Task<bool> CanManageProjectAsync(string userId, int projectId)
    {
        var project = await _db.Projects.Include(p => p.Team).FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return false;
        return await CanManageTeamAsync(userId, project.TeamId);
    }

    public async Task<bool> CanViewProjectAsync(string userId, int projectId)
    {
        var project = await _db.Projects
            .Include(p => p.Team)
            .ThenInclude(t => t!.Members)
            .FirstOrDefaultAsync(p => p.Id == projectId);
        if (project == null) return false;
        var inTeam = project.Team.Members.Any(m => m.UserId == userId);
        if (inTeam) return true;
        var role = await GetUserRoleInOrganizationAsync(userId, project.Team.OrganizationId);
        return role.HasValue;
    }

    public async Task<WorkspaceRole?> GetUserRoleInOrganizationAsync(string userId, int organizationId)
    {
        var m = await _db.WorkspaceMemberships
            .FirstOrDefaultAsync(w => w.OrganizationId == organizationId && w.UserId == userId);
        return m?.Role;
    }
}
