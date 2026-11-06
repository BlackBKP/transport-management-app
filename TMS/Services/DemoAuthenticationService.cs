using Microsoft.AspNetCore.Identity;
using TMS.Interfaces;

namespace TMS.Services
{
    // Fixed demo accounts; passwords are verified against salted PBKDF2 hashes.
    public class DemoAuthenticationService : IDemoAuthenticationService
    {
        private const string UserPasswordHash =
            "AQAAAAIAAYagAAAAEEhqdHWUCHd9zfSOFi6uktn+d6CGE8Q9ong4kUlz0h+o33mYEHmQk4p4ZymtpdpkNQ==";

        private const string KorakodPasswordHash =
            "AQAAAAIAAYagAAAAEOIdE0fB0WxDGkyYx9BH1+Pr+57+uYLSYo33xaG7tIznzXmgIYa7lSjxF+xm2YPnBg==";

        private static readonly Dictionary<string, string> PasswordHashes =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["user"] = UserPasswordHash,
                ["korakod.p"] = KorakodPasswordHash
            };

        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();

        public string? Authenticate(string userName, string password)
        {
            string normalizedName = userName.Trim().ToLowerInvariant();
            bool knownUser = PasswordHashes.TryGetValue(normalizedName, out string? passwordHash);

            // Unknown users still incur password verification rather than returning early.
            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
                normalizedName,
                passwordHash ?? UserPasswordHash,
                password);

            if (!knownUser || result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return normalizedName;
        }
    }
}
