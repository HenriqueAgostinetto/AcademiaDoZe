// henrique agostinetto piva
using System.Security.Cryptography;
using System.Text;
using Konscious.Security.Cryptography;

namespace AcademiaDoZe.Application.Security;

public static class PasswordHasher
{
    public static string Hash(string password)
    {
        if (string.IsNullOrWhiteSpace(password)) throw new ArgumentException("Senha obrigatoria", nameof(password));
        var salt = RandomNumberGenerator.GetBytes(16);
        var parallelism = Math.Max(1, Environment.ProcessorCount);
        var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)) { Salt = salt, Iterations = 3, MemorySize = 65536, DegreeOfParallelism = parallelism };
        return $"ARGON2ID:3:65536:{parallelism}:{Convert.ToBase64String(salt)}:{Convert.ToBase64String(argon2.GetBytes(32))}";
    }

    public static bool Verify(string password, string passwordHash)
    {
        var parts = passwordHash.Split(':');
        if (string.IsNullOrWhiteSpace(password) || parts.Length != 6 || parts[0] != "ARGON2ID" || !int.TryParse(parts[1], out var iterations) || !int.TryParse(parts[2], out var memory) || !int.TryParse(parts[3], out var parallelism)) return false;
        try { var expected = Convert.FromBase64String(parts[5]); var argon2 = new Argon2id(Encoding.UTF8.GetBytes(password)) { Salt = Convert.FromBase64String(parts[4]), Iterations = iterations, MemorySize = memory, DegreeOfParallelism = Math.Max(1, parallelism) }; return CryptographicOperations.FixedTimeEquals(argon2.GetBytes(expected.Length), expected); } catch { return false; }
    }
}
