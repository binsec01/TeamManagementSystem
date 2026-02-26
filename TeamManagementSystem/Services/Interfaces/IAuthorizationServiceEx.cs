using TeamManagementSystem.Web.Models;

namespace TeamManagementSystem.Web.Services.Interfaces;

public interface IAuthorizationServiceEx
{
    Task<bool> CanManageOrganizationAsync(string userId, int organizationId);
    Task<bool> CanManageTeamAsync(string userId, int teamId);
    Task<bool> CanManageProjectAsync(string userId, int projectId);
    Task<bool> CanViewProjectAsync(string userId, int projectId);
    Task<WorkspaceRole?> GetUserRoleInOrganizationAsync(string userId, int organizationId);
}
