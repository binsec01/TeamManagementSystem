using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Organizations;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class TeamsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;

    public TeamsController(AppDbContext db, IAuthorizationServiceEx authEx)
    {
        _db = db;
        _authEx = authEx;
    }

    public async Task<IActionResult> Index(int organizationId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (!role.HasValue) return Forbid();

        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (org == null) return NotFound();

        var teams = await _db.Teams
            .Include(t => t.Projects)
            .Include(t => t.Members)
            .Where(t => t.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);

        ViewData["OrganizationId"] = organizationId;
        ViewData["OrganizationName"] = org.Name;
        ViewData["CanManage"] = role is WorkspaceRole.Admin or WorkspaceRole.Lead;
        return View(teams);
    }

    [HttpGet]
    public async Task<IActionResult> Create(int organizationId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (role is not WorkspaceRole.Admin and not WorkspaceRole.Lead) return Forbid();

        ViewData["OrganizationId"] = organizationId;
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(int organizationId, string name, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (role is not WorkspaceRole.Admin and not WorkspaceRole.Lead) return Forbid();

        _db.Teams.Add(new Team { OrganizationId = organizationId, Name = name ?? "New Team" });
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index), new { organizationId });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var team = await _db.Teams
            .Include(t => t.Organization)
            .Include(t => t.Projects)
            .Include(t => t.Members)
            .ThenInclude(m => m.User)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);
        if (team == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var orgRole = await _authEx.GetUserRoleInOrganizationAsync(user.Id, team.OrganizationId);
        if (!orgRole.HasValue) return Forbid();

        ViewData["CanManage"] = orgRole is WorkspaceRole.Admin or WorkspaceRole.Lead;
        return View(team);
    }
}
