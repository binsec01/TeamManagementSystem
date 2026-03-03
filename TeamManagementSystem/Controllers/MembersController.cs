using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TeamManagementSystem.Web.Data;
using TeamManagementSystem.Web.Models;
using TeamManagementSystem.Web.Models.Identity;
using TeamManagementSystem.Web.Services.Interfaces;
using TeamManagementSystem.Web.ViewModels.Members;

namespace TeamManagementSystem.Web.Controllers;

[Authorize]
public class MembersController : Controller
{
    private readonly AppDbContext _db;
    private readonly IAuthorizationServiceEx _authEx;
    private readonly UserManager<ApplicationUser> _userManager;

    public MembersController(
        AppDbContext db,
        IAuthorizationServiceEx authEx,
        UserManager<ApplicationUser> userManager)
    {
        _db = db;
        _authEx = authEx;
        _userManager = userManager;
    }

    public async Task<IActionResult> Index(int organizationId, CancellationToken cancellationToken)
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, organizationId);
        if (!role.HasValue) return Forbid();

        var org = await _db.Organizations.AsNoTracking()
            .FirstOrDefaultAsync(o => o.Id == organizationId, cancellationToken);
        if (org == null) return NotFound();

        var memberships = await _db.WorkspaceMemberships
            .Include(m => m.User)
            .Where(m => m.OrganizationId == organizationId)
            .OrderBy(m => m.Role)
            .ThenBy(m => m.User.FullName ?? m.User.Email)
            .ToListAsync(cancellationToken);

        var invites = await _db.WorkspaceInvites
            .Where(i => i.OrganizationId == organizationId
                        && !i.IsRevoked
                        && i.AcceptedAtUtc == null
                        && (i.ExpiresAtUtc == null || i.ExpiresAtUtc > DateTime.UtcNow))
            .OrderByDescending(i => i.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        var vm = new OrganizationMembersVm
        {
            OrganizationId = org.Id,
            OrganizationName = org.Name,
            CurrentUserRole = role.Value,
            Members = memberships.Select(m => new OrganizationMemberRowVm
            {
                MembershipId = m.Id,
                UserId = m.UserId,
                DisplayName = m.User.FullName ?? m.User.Email ?? m.UserId,
                Email = m.User.Email ?? string.Empty,
                Role = m.Role,
                JoinedAtUtc = m.JoinedAtUtc
            }).ToList(),
            PendingInvites = invites.Select(i => new OrganizationInviteRowVm
            {
                Id = i.Id,
                Email = i.Email,
                Role = i.Role,
                Code = i.Code,
                CreatedAtUtc = i.CreatedAtUtc,
                ExpiresAtUtc = i.ExpiresAtUtc
            }).ToList()
        };

        ViewData["CanManage"] = role is WorkspaceRole.Admin or WorkspaceRole.Lead;
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> CreateInvite(CreateInviteVm model, CancellationToken cancellationToken)
    {
        if (!ModelState.IsValid)
        {
            return await Index(model.OrganizationId, cancellationToken);
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null) return RedirectToAction("Login", "Account");

        var role = await _authEx.GetUserRoleInOrganizationAsync(user.Id, model.OrganizationId);
        if (role is not WorkspaceRole.Admin and not WorkspaceRole.Lead) return Forbid();

        var org = await _db.Organizations.FirstOrDefaultAsync(o => o.Id == model.OrganizationId, cancellationToken);
        if (org == null) return NotFound();

        var invite = new WorkspaceInvite
        {
            OrganizationId = org.Id,
            Email = string.IsNullOrWhiteSpace(model.Email) ? null : model.Email.Trim(),
            Role = model.Role,
            CreatedById = user.Id,
            CreatedAtUtc = DateTime.UtcNow,
            ExpiresAtUtc = DateTime.UtcNow.AddDays(7)
        };

        _db.WorkspaceInvites.Add(invite);
        await _db.SaveChangesAsync(cancellationToken);

        var inviteUrl = Url.Action(
            "Join",
            "Members",
            new { code = invite.Code },
            protocol: Request.Scheme,
            host: Request.Host.ToString());

        TempData["LastInviteLink"] = inviteUrl ?? string.Empty;

        return RedirectToAction(nameof(Index), new { organizationId = model.OrganizationId });
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> Join(string code, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(code)) return BadRequest();

        var invite = await _db.WorkspaceInvites
            .Include(i => i.Organization)
            .FirstOrDefaultAsync(i => i.Code == code, cancellationToken);
        if (invite == null || invite.IsRevoked ||
            invite.AcceptedAtUtc != null ||
            (invite.ExpiresAtUtc.HasValue && invite.ExpiresAtUtc.Value < DateTime.UtcNow))
        {
            return View("InvalidInvite");
        }

        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
            var returnUrl = Url.Action("Join", "Members", new { code }, Request.Scheme, Request.Host.ToString());
            return RedirectToAction("Login", "Account", new { returnUrl });
        }

        var membership = await _db.WorkspaceMemberships
            .FirstOrDefaultAsync(m => m.OrganizationId == invite.OrganizationId && m.UserId == user.Id, cancellationToken);

        if (membership == null)
        {
            _db.WorkspaceMemberships.Add(new WorkspaceMembership
            {
                OrganizationId = invite.OrganizationId,
                UserId = user.Id,
                Role = invite.Role,
                JoinedAtUtc = DateTime.UtcNow
            });
        }

        invite.AcceptedAtUtc = DateTime.UtcNow;
        invite.AcceptedByUserId = user.Id;

        await _db.SaveChangesAsync(cancellationToken);

        return RedirectToAction("Details", "Organizations", new { id = invite.OrganizationId });
    }
}

