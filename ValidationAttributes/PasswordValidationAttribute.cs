using System.ComponentModel.DataAnnotations;

namespace SelfHostedPasswordManager.Models.ValidationAttributes
{
    public class PasswordValidationAttribute : ValidationAttribute
    {
        public override bool IsValid(object value)
        {
            if (value == null)
                return false;

            string password = value.ToString();

            if (password.Length < 16)
                return false;

            if (!password.Any(char.IsUpper))
                return false;

            if (!password.Any(char.IsLower))
                return false;

            if (!password.Any(char.IsDigit))
                return false;

            if (!password.Any(c => !char.IsLetterOrDigit(c)))
                return false;

            return true;
        }
    }
}
