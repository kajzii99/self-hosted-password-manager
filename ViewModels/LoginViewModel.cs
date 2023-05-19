using System.ComponentModel.DataAnnotations;
using SelfHostedPasswordManager.Models.ValidationAttributes;

namespace SelfHostedPasswordManager.ViewModels
{
    public class LoginViewModel
    {
        [Required] 
        [EmailAddress]
        [Display(Name = "E-mail address")]
        public string Email { get; set; }   

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [PasswordValidation(ErrorMessage = "Login attempt failed! Please check your e-mail and password")]
        public string Password { get; set; }

        [Display(Name = "Remember me")]
        public bool RememberMe { get; set; }
    }
}
