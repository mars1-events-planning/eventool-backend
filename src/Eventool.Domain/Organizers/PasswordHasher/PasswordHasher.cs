using System.Security.Cryptography;

namespace Eventool.Domain.Organizers;

public class PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int KeySize = 32;
    private const int Iterations = 50000;
    private static readonly HashAlgorithmName Algorithm = HashAlgorithmName.SHA256;

    public HashedPassword Hash(string password)
    {
        var salt = RandomNumberGenerator.GetBytes(SaltSize);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );
        return new HashedPassword(
            Convert.ToBase64String(hash), Convert.ToBase64String(salt));
    }
    
    public bool IsSame(string password, HashedPassword hashedPassword)
    {
        var salt = Convert.FromBase64String(hashedPassword.Salt);
        var hash = Rfc2898DeriveBytes.Pbkdf2(
            password,
            salt,
            Iterations,
            Algorithm,
            KeySize
        );
        return hashedPassword.Value == Convert.ToBase64String(hash);
    }
}