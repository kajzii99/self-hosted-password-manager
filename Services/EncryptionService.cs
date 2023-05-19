using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace SelfHostedPasswordManager.Encryption 
{
    public class EncryptionService
    {
        private readonly byte[] key;
        private readonly byte[] iv;

        public EncryptionService(IConfiguration configuration)
        {
            this.key = Encoding.UTF8.GetBytes(configuration["Encryption:Key"]);
            this.iv = Encoding.UTF8.GetBytes(configuration["Encryption:IV"]);
        }

        public string Encrypt(string data)
        {
            byte[] encrypted;

            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = key;
                aesAlg.IV = iv;

                ICryptoTransform encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

                using (MemoryStream msEncrypt = new MemoryStream())
                {
                    using (CryptoStream csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
                    {
                        using (StreamWriter swEncrypt = new StreamWriter(csEncrypt))
                            swEncrypt.Write(data);

                        encrypted = msEncrypt.ToArray();
                    }
                }
            }

            return Convert.ToBase64String(encrypted);
        }

        public string Decrypt(string data)
        {
            try
            {
                data = data.Trim();

                if (!(data.Length % 4 == 0) && Regex.IsMatch(data, @"^[a-zA-Z0-9\+/]*={0,3}$", RegexOptions.None))
                    return string.Empty;


                byte[] encrypted = Convert.FromBase64String(data);
                string decryptedData;

                using (Aes aesAlg = Aes.Create())
                {
                    aesAlg.Key = key;
                    aesAlg.IV = iv;

                    ICryptoTransform decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

                    using (MemoryStream msDecrypt = new MemoryStream(encrypted))
                    {
                        using (CryptoStream csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read))
                        {
                            using (StreamReader srDecrypt = new StreamReader(csDecrypt))
                            {
                                decryptedData = srDecrypt.ReadToEnd();
                            }
                        }
                    }
                }
                return decryptedData;
            }
            catch (Exception)
            {
                return string.Empty;
            }
        }
    }
}

