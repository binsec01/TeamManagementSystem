using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Projects;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class ProjectsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;

    public ProjectsController(AppDbContext db, IAuthorizationServiceEx authEx)
    {
        _db = db;
        _authEx = authEx;
    }

    public async Task<IActionResult> Index(int teamId, CancellationToken cancellationToken)
    {
        var team = await _db.Teams.Include(t => t.Organization).FirstOrDefaultAsync(t => t.Id == teamId, cancellationToken);
        if (team == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, team.OrganizationId);
        if (!role.HasValue) return Forbid();

        var projects = await _db.Projects
            .Include(p => p.Tasks)
            .Where(p => p.TeamId == teamId)
            .ToListAsync(cancellationToken);

        ViewData["TeamId"] = teamId;
        ViewData["TeamName"] = team.Name;
        ViewData["OrganizationId"] = team.OrganizationId;
        ViewData["CanManage"] = await _authEx.CanManageTeamAsync(user.Id, teamId);
        return View(projects);
    }

    [HttpGet]
    public IActionResult Create(int teamId)
    {
        ViewData["TeamId"] = teamId;
        return View(new ProjectCreateVm { TeamId = teamId });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ProjectCreateVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            ViewData["TeamId"] = model.TeamId;
            return View(model);
        }
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanManageTeamAsync(user.Id, model.TeamId)) return Forbid();

        _db.Projects.Add(new Project
        {
            TeamId = model.TeamId,
            Name = model.Name,
            Description = model.Description,
            StartDateUtc = model.StartDateUtc,
            DueDateUtc = model.DueDateUtc
        });
        await _db.SaveChangesAsync(cancellationToken);
        return RedirectToAction(nameof(Index), new { teamId = model.TeamId });
    }

    public async Task<IActionResult> Details(int id, CancellationToken cancellationToken)
    {
        var project = await _db.Projects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .FirstOrDefaultAsync(p => p.Id == id, cancellationToken);
        if (project == null) return NotFound();

        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        if (!await _authEx.CanViewProjectAsync(user.Id, id)) return Forbid();

        var now = DateTime.UtcNow;
        var vm = new ProjectDetailsVm
        {
            Id = project.Id,
            TeamId = project.TeamId,
            TeamName = project.Team.Name,
            Name = project.Name,
            Description = project.Description,
            StartDateUtc = project.StartDateUtc,
            DueDateUtc = project.DueDateUtc,
            OpenTaskCount = project.Tasks.Count(t => t.Status != Models.TaskStatus.Done),
            CompletedTaskCount = project.Tasks.Count(t => t.Status == Models.TaskStatus.Done),
            OverdueTaskCount = project.Tasks.Count(t => t.DueDateUtc.HasValue && t.DueDateUtc < now && t.Status != Models.TaskStatus.Done)
        };
        return View(vm);
    }
}
