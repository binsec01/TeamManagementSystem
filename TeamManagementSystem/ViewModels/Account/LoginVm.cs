using System.ComponentModel.DataAnnotations;

namespace TeamManagementSystem.Web.ViewModels.Account;

public class LoginVm
{
    [Required]
    [EmailAddress]
    [Display(Name = "Email")]
    public string Email { get; set; } = default!;

    [Required]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string Password { get; set; } = default!;

    [Display(Name = "Remember me?")]
    public bool RememberMe { get; set; }
}
