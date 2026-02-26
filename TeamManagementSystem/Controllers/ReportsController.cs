using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Reports;
using TaskStatusEnum = TeamManagementSystem.Web.Models.TaskStatus;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class ReportsController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;

    public ReportsController(AppDbContext db, IAuthorizationServiceEx authEx)
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
        ViewData["OrganizationId"] = organizationId;
        ViewData["OrganizationName"] = org.Name;
        return View();
    }

    public async Task<IActionResult> ProjectProgress(int organizationId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (!role.HasValue) return Forbid();

        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (org == null) return NotFound();

        var projects = await _db.Projects
            .Include(p => p.Team)
            .Include(p => p.Tasks)
            .Where(p => p.Team.OrganizationId == organizationId)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var vm = new ProjectProgressReportVm
        {
            OrganizationId = organizationId,
            OrganizationName = org.Name,
            Projects = projects.Select(p => new ProjectProgressRowVm
            {
                ProjectId = p.Id,
                ProjectName = p.Name,
                TeamName = p.Team.Name,
                OpenTasks = p.Tasks.Count(t => t.Status != TaskStatusEnum.Done),
                CompletedTasks = p.Tasks.Count(t => t.Status == TaskStatusEnum.Done),
                OverdueTasks = p.Tasks.Count(t => t.DueDateUtc.HasValue && t.DueDateUtc < now && t.Status != TaskStatusEnum.Done),
                TotalTasks = p.Tasks.Count
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> TeamWorkload(int organizationId, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (!role.HasValue) return Forbid();

        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (org == null) return NotFound();

        var memberships = await _db.WorkspaceMemberships
            .Include(w => w.User)
            .Where(w => w.OrganizationId == organizationId)
            .Select(w => w.UserId)
            .Distinct()
            .ToListAsync(cancellationToken);
        var tasksByUser = await _db.Tasks
            .Include(t => t.Project)
            .ThenInclude(p => p!.Team)
            .Where(t => t.Project.Team.OrganizationId == organizationId && t.AssigneeId != null)
            .ToListAsync(cancellationToken);
        var now = DateTime.UtcNow;
        var grouped = tasksByUser.GroupBy(t => t.AssigneeId!).ToDictionary(g => g.Key, g => g.ToList());
        var users = await _db.Users.Where(u => memberships.Contains(u.Id)).ToDictionaryAsync(u => u.Id, u => u, cancellationToken);
        var vm = new TeamWorkloadReportVm
        {
            OrganizationId = organizationId,
            OrganizationName = org.Name,
            Members = grouped.Select(kv => new MemberWorkloadRowVm
            {
                UserId = kv.Key,
                FullName = users.GetValueOrDefault(kv.Key)?.FullName ?? "",
                Email = users.GetValueOrDefault(kv.Key)?.Email ?? "",
                TotalTasks = kv.Value.Count,
                OpenTasks = kv.Value.Count(t => t.Status != TaskStatusEnum.Done),
                InProgressTasks = kv.Value.Count(t => t.Status == TaskStatusEnum.InProgress),
                CompletedTasks = kv.Value.Count(t => t.Status == TaskStatusEnum.Done),
                OverdueTasks = kv.Value.Count(t => t.DueDateUtc.HasValue && t.DueDateUtc < now && t.Status != TaskStatusEnum.Done)
            }).ToList()
        };
        return View(vm);
    }

    public async Task<IActionResult> Activity(int organizationId, DateTime? from, DateTime? to, CancellationToken cancellationToken)
    {
        var userId = User.Identity?.Name ?? "";
        var user = await _db.Users.FirstOrDefaultAsync(u => u.UserName == userId, cancellationToken);
        if (user == null) return RedirectToAction("Login", "Account");
        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (!role.HasValue) return Forbid();

        var org = await _db.Organizations.AsNoTracking().FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (org == null) return NotFound();

        var teamIds = await _db.Teams.Where(t => t.OrganizationId == organizationId).Select(t => t.Id).ToListAsync(cancellationToken);
        var projectIds = await _db.Projects.Where(p => teamIds.Contains(p.TeamId)).Select(p => p.Id).ToListAsync(cancellationToken);
        var allLogs = await _db.ActivityLogs.AsNoTracking()
            .OrderByDescending(a => a.CreatedAtUtc)
            .Take(500)
            .ToListAsync(cancellationToken);
        var taskIdsInOrg = await _db.Tasks.Where(t => projectIds.Contains(t.ProjectId)).Select(t => t.Id).ToListAsync(cancellationToken);
        var taskIdSet = taskIdsInOrg.ToHashSet();
        allLogs = allLogs.Where(a => a.EntityType != "TaskItem" || (int.TryParse(a.EntityId, out var tid) && taskIdSet.Contains(tid))).Take(200).ToList();
        var userDict = await _db.Users.ToDictionaryAsync(u => u.Id, u => u.FullName ?? u.Email ?? u.Id, cancellationToken);
        var vm = new ActivityReportVm
        {
            OrganizationId = organizationId,
            OrganizationName = org.Name,
            FromUtc = from,
            ToUtc = to,
            Activities = allLogs.Select(a => new ActivityRowVm
            {
                Id = a.Id,
                ActorUserId = a.ActorUserId,
                ActorName = userDict.GetValueOrDefault(a.ActorUserId),
                EntityType = a.EntityType,
                EntityId = a.EntityId,
                Action = a.Action,
                DetailsJson = a.DetailsJson,
                CreatedAtUtc = a.CreatedAtUtc
            }).ToList()
        };
        return View(vm);
    }
}
