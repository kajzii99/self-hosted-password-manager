using System;
using System.Text;

namespace SelfHostedPasswordManager.Services
{
    public class PasswordGeneratorService
    {
        private readonly string alphabetLowercase;
        private readonly string alphabetUppercase;
        private readonly string numbers;
        private readonly string specialCharacters;

        private readonly int minimumLength;
        private readonly int maximumLength;

        public PasswordGeneratorService(IConfiguration configuration)
        {
            this.alphabetLowercase = configuration["PasswordGenerator:AlphabetLowercase"];
            this.alphabetUppercase = configuration["PasswordGenerator:AlphabetUppercase"];
            this.numbers = configuration["PasswordGenerator:Numbers"];
            this.specialCharacters = configuration["PasswordGenerator:SpecialCharacters"];

            this.minimumLength = Convert.ToInt32(configuration["PasswordGenerator:MinimumLength"]);
            this.maximumLength = Convert.ToInt32(configuration["PasswordGenerator:MaximumLength"]);
        }

        public string GeneratePassword()
        {
            Random rnd = new Random();
            int passwdLen = rnd.Next(minimumLength, maximumLength); // 16 - jeśli zostanie wylosowana najniższa możliwa długość, to podzielona przez 8 będzie równa 2, co oznacza, że finalne hasło będzie zbudowane z: 2*3+2*2+2*2+2*1 = 16 
            int part = passwdLen / 8;

            string lowercasePart = GeneratePasswordPart(alphabetLowercase, part * 3);       //    3/8 na małe znaki
            string uppercasePart = GeneratePasswordPart(alphabetUppercase, part * 2);      //    2/8 na duże znaki
            string specialCharsPart = GeneratePasswordPart(specialCharacters, part * 2);  //    2/8 na znaki specjalne
            string numbersPart = GeneratePasswordPart(numbers, part * 1);                //    1/8 na cyfry

            string shuffledPassword = Shuffle(lowercasePart + uppercasePart + specialCharsPart + numbersPart);

            if (shuffledPassword.Length < minimumLength)
                return GeneratePassword();

            return shuffledPassword;
        }

        private string Shuffle(string data)
        {
            char[] charArray = data.ToCharArray();
            Random rng = new Random();
            int n = charArray.Length;
            while (n > 1)
            {
                n--;
                int k = rng.Next(n + 1);
                var value = charArray[k];
                charArray[k] = charArray[n];
                charArray[n] = value;
            }
            return new string(charArray);
        }

        private string GeneratePasswordPart(string alphabet, int length)
        {
            StringBuilder sb = new StringBuilder();
            Random rnd = new Random();

            while (0 < length--)
                sb.Append(alphabet[rnd.Next(alphabet.Length)]);

            return sb.ToString();
        }
    }
}