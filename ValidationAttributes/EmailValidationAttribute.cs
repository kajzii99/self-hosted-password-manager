using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;

namespace SelfHostedPasswordManager.Models.ValidationAttributes
{
    public class EmailValidationAttribute : ValidationAttribute
    {
        protected override ValidationResult IsValid(object value, ValidationContext validationContext)
        {
            if (value != null)
            {
                string email = value.ToString();
                string pattern = @"^([\w\.\-]+)@([\w\-]+)((\.(\w){2,3})+)$"; //regex
                Match match = Regex.Match(email, pattern);
                if (!match.Success)
                {
                    return new ValidationResult("Wrong e-mail address");
                }
            }
            return ValidationResult.Success;
        }
    }
}