using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using TeamManagementSystem.Web.Services.Interfaces;

namespace TeamManagementSystem.Web.Policies;

public class OrganizationPermissionRequirement : IAuthorizationRequirement
{
    public string Permission { get; }

    public OrganizationPermissionRequirement(string permission)
    {
        Permission = permission;
    }
}

public class PermissionHandler : AuthorizationHandler<OrganizationPermissionRequirement, int>
{
    private readonly IAuthorizationServiceEx _authEx;

    public PermissionHandler(IAuthorizationServiceEx authEx)
    {
        _authEx = authEx;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        OrganizationPermissionRequirement requirement,
        int resource)
    {
        var userId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(userId)) return;

        var allowed = requirement.Permission switch
        {
            PermissionConstants.ManageOrganization => await _authEx.CanManageOrganizationAsync(userId, resource),
            PermissionConstants.ViewProject => await _authEx.CanViewProjectAsync(userId, resource),
            PermissionConstants.ManageTeam => await _authEx.CanManageTeamAsync(userId, resource),
            PermissionConstants.ManageProject => await _authEx.CanManageProjectAsync(userId, resource),
            _ => false
        };
        if (allowed) context.Succeed(requirement);
    }
}
