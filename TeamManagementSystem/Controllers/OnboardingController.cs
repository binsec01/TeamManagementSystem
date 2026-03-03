using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Models.Identity;
using TeamManagementSystem.Web.Policies;
using TeamManagementSystem.Web.ViewModels.Onboarding;
using TeamManagementSystem.Web.Services;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class OnboardingController : Controller
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public OnboardingController(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _userManager = userManager;
    }

    [HttpGet]
    public async Task<IActionResult> Index(CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        return View();
    }

    [HttpPost]
    public async Task<IActionResult> CreateOrganization([FromBody] CreateOrganizationStepVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var org = new Organization { Name = model.Name };
        _db.Organizations.Add(org);
        await _db.SaveChangesAsync(cancellationToken);

        var membership = new WorkspaceMembership
        {
            OrganizationId = org.Id,
            UserId = user.Id,
            Role = WorkspaceRole.Admin,
            JoinedAtUtc = DateTime.UtcNow
        };
        _db.WorkspaceMemberships.Add(membership);

        var team = new Team
        {
            OrganizationId = org.Id,
            Name = string.IsNullOrWhiteSpace(model.DefaultTeamName) ? "General" : model.DefaultTeamName
        };
        _db.Teams.Add(team);

        await _db.SaveChangesAsync(cancellationToken);

        return Json(new { organizationId = org.Id, teamId = team.Id });
    }

    [HttpPost]
    public async Task<IActionResult> CreateProject([FromBody] CreateProjectStepVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == model.TeamId && t.OrganizationId == model.OrganizationId, cancellationToken);
        if (team == null) return BadRequest("Invalid team or organization.");

        var project = new Project
        {
            TeamId = team.Id,
            Name = model.Name,
            Description = model.Description
        };
        _db.Projects.Add(project);
        await _db.SaveChangesAsync(cancellationToken);

        return Json(new { projectId = project.Id });
    }

    [HttpPost]
    public async Task<IActionResult> ApplyTemplate([FromBody] ApplyTemplateStepVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid) return BadRequest(ModelState);

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == model.ProjectId, cancellationToken);
        if (project == null) return BadRequest("Invalid project.");

        var tasks = OnboardingTemplates.CreateTemplateTasks(model.TemplateKey, project.Id, user.Id);
        if (tasks.Count > 0)
        {
            _db.Tasks.AddRange(tasks);
            await _db.SaveChangesAsync(cancellationToken);
        }

        return Ok();
    }

    [HttpPost]
    public async Task<IActionResult> InviteTeam([FromBody] InviteTeamStepVm model, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return Unauthorized();

        var project = await _db.Projects.FirstOrDefaultAsync(p => p.Id == model.ProjectId, cancellationToken);
        if (project == null) return BadRequest("Invalid project.");

        var team = await _db.Teams.FirstOrDefaultAsync(t => t.Id == project.TeamId, cancellationToken);
        if (team == null) return BadRequest("Invalid team.");

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == team.OrganizationId, cancellationToken);
        if (org == null) return BadRequest("Invalid organization.");

        if (!ModelState.IsValid) return BadRequest(ModelState);

        foreach (var entry in model.Invites)
        {
            if (string.IsNullOrWhiteSpace(entry.Email)) continue;

            var targetRole = entry.Role?.Equals("Manager", StringComparison.OrdinalIgnoreCase) == true
                ? WorkspaceRole.Lead
                : WorkspaceRole.Member;

            var invite = new WorkspaceInvite
            {
                OrganizationId = org.Id,
                Email = entry.Email.Trim(),
                Role = targetRole,
                CreatedById = user.Id,
                CreatedAtUtc = DateTime.UtcNow,
                ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
            };

            _db.WorkspaceInvites.Add(invite);
        }

        await _db.SaveChangesAsync(cancellationToken);

        return Ok();
    }
}

