using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Onboarding;

public class ApplyTemplateStepVm
{
    [Required]
    public int ProjectId { get; set; }

    [Required]
    [MaxLength(50)]
    public string TemplateKey { get; set; } = default!;
}

