using System.Security.Cryptography;
using System.Text;

namespace Http_Server;

public class HashPassword
{
    public static string Hash(string password)
    {
        using (var sha256 = SHA256.Create())
        {
            var hasheBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(password));
            var hash = BitConverter.ToString(hasheBytes).Replace("-", "").ToLower();
            return hash;
        }
    }
}