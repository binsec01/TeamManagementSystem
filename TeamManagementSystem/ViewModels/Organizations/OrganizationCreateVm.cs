using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Organizations;

public class OrganizationCreateVm
{
    [Required]
    [StringLength(200)]
    [Display(Name = "Organization Name")]
    public string Name { get; set; } = default!;
}
