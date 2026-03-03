using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Onboarding;

public class InviteTeamStepVm
{
    [Required]
    public int ProjectId { get; set; }

    public List<InviteEntryVm> Invites { get; set; } = new();
}

public class InviteEntryVm
{
    [Required]
    [EmailAddress]
    public string Email { get; set; } = default!;

    [MaxLength(50)]
    public string? Role { get; set; }
}

