using System.ComponentModel.DataAnnotations;
using SelfHostedPasswordManager.Models.ValidationAttributes;

namespace SelfHostedPasswordManager.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Email address is required!")]
        [Display(Name = "E-mail Address")]
        [EmailValidationAttribute(ErrorMessage = "Incorrect e-mail address!")]
        public string Email { get; set; }

        [Required]
        [DataType(DataType.Password)]
        [Display(Name = "Password")]
        [PasswordValidationAttribute(ErrorMessage = "Incorrect password! The password should contain at least 12 characters, one uppercase letter, one lowercase letter, a special character and a number")]
        public string Password { get; set; }

        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords aren't the same")]
        public string ConfirmPassword { get; set; }
        /*public bool RememberMe { get; set; }*/    
    }
}
