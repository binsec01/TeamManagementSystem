using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Policies;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Organizations;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class OrganizationsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;

    public OrganizationsController(AppDbContext db, IAuthorizationServiceEx authEx)
    {
        _db = db;
        _authEx = authEx;
    }

    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");

        var memberships = await _db.WorkspaceMemberships
            .Include(w => w.Organization)
            .Where(w => w.UserId == user.Id)
            .ToListAsync(cancellationToken);

        var orgIds = memberships.Select(m => m.OrganizationId).Distinct().ToList();
        var teamRows = await _db.Teams
            .Where(t => orgIds.Contains(t.OrganizationId))
            .Select(t => new { t.OrganizationId, t.Id })
            .ToListAsync(cancellationToken);
        var firstTeamByOrg = teamRows
            .GroupBy(t => t.OrganizationId)
            .ToDictionary(g => g.Key, g => g.Min(x => x.Id));

        var orgs = memberships.Select(m => new OrganizationListVm
        {
            Organization = m.Organization,
            Role = m.Role,
            FirstTeamId = firstTeamByOrg.TryGetValue(m.OrganizationId, out var teamId) ? teamId : null
        }).ToList();
        return View(orgs);
    }

    [HttpGet]
    public IActionResult Create() => View(new OrganizationCreateVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(OrganizationCreateVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return View(model);
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");

        var org = new Organization { Name = model.Name };
        _db.Organizations.Add(org);
        await _db.SaveChangesAsync(cancellationToken);
        _db.WorkspaceMemberships.Add(new WorkspaceMembership
        {
            OrganizationId = org.Id,
            UserId = user.Id,
            Role = WorkspaceRole.Admin
        });
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Details), new { id = org.Id });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");

        var canAccess = await _authEx.GetUserRoleInOrganizationAsync(user.Id, id);
        if (!canAccess.HasValue) return Forbid();

        var org = await _db.Organizations
            .Include(o => o.Teams)
            .ThenInclude(t => t.Projects)
            .Include(o => o.Teams)
            .ThenInclude(t => t.Members)
            .FirstOrDefaultAsync(o => o.Id == id, cancellationToken);
        if (org == null) return NotFound();

        var teams = org.Teams.Select(t => new TeamSummaryVm
        {
            Id = t.Id,
            Name = t.Name,
            ProjectCount = t.Projects.Count,
            MemberCount = t.Members.Count
        }).ToList();

        var vm = new OrganizationDetailsVm
        {
            Id = org.Id,
            Name = org.Name,
            CreatedAtUtc = org.CreatedAtUtc,
            Teams = teams,
            CurrentUserRole = canAccess.Value.ToString()
        };
        return View(vm);
    }
}
