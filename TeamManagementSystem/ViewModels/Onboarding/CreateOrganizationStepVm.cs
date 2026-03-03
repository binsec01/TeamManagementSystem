using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Onboarding;

public class CreateOrganizationStepVm
{
    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(200)]
    public string? Industry { get; set; }

    [MaxLength(200)]
    public string? DefaultTeamName { get; set; } = "General";
}

