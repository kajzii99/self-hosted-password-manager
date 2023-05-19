using SelfHostedPasswordManager.Models.ValidationAttributes;
using System.ComponentModel.DataAnnotations;

namespace SelfHostedPasswordManager.Models

{
    public class Credential
    { 
        public string Id { get; set; }
        
        public string Website { get; set; }
        
        public string Username { get; set; }
        [Required(ErrorMessage = "Password is required.")]
        [PasswordValidationAttribute(ErrorMessage = 
            "Password should be composed of a minimum of 12 characters, " +
            "should contain uppercase and lowercase letters, " +
            "special characters and numbers.")]
        //[DataType(DataType.Password)]
        public string Password { get; set; }
        public string? Notes { get; set; }
        public string? ApplicationUserId { get; set; }

        public Credential()
        {
            this.Id = Guid.NewGuid().ToString();
            Console.WriteLine(Id);
        }
    }
}
