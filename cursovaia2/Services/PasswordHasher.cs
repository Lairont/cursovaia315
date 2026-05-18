using System.Security.Cryptography;
using System.Text;

namespace cursovaia2.Services
{
    public static class PasswordHasher
    {
        public static string Hash(string password)
        {
            var hashedBytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToBase64String(hashedBytes);
        }

        public static bool Verify(string password, string hash) =>
            Hash(password) == hash;
    }
}
