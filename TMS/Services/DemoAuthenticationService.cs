using Microsoft.AspNetCore.Identity;
using TMS.Interfaces;

namespace TMS.Services
{
    // Fixed accounts for presenting the prototype. Replace with an identity provider for production.
    public class DemoAuthenticationService : IDemoAuthenticationService
    {
        private readonly PasswordHasher<string> _passwordHasher = new PasswordHasher<string>();
        private readonly Dictionary<string, string> _passwordHashes;
        private readonly string _dummyHash;

        public DemoAuthenticationService()
        {
            _passwordHashes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["user"] = _passwordHasher.HashPassword("user", "P@ssword"),
                ["korakod.p"] = _passwordHasher.HashPassword("korakod.p", "P@ssw0rd")
            };
            _dummyHash = _passwordHasher.HashPassword("unknown", Guid.NewGuid().ToString());
        }

        public string? Authenticate(string userName, string password)
        {
            string normalizedName = userName.Trim().ToLowerInvariant();
            bool knownUser = _passwordHashes.TryGetValue(normalizedName, out string? passwordHash);
            PasswordVerificationResult result = _passwordHasher.VerifyHashedPassword(
                normalizedName,
                passwordHash ?? _dummyHash,
                password);

            if (!knownUser || result == PasswordVerificationResult.Failed)
            {
                return null;
            }

            return normalizedName;
        }
    }
}
