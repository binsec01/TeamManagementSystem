using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Onboarding;

public class CreateProjectStepVm
{
    [Required]
    public int OrganizationId { get; set; }

    [Required]
    public int TeamId { get; set; }

    [Required]
    [MaxLength(200)]
    public string Name { get; set; } = default!;

    [MaxLength(1000)]
    public string? Description { get; set; }
}

