using Hive.Application.Abstractions.Security;
using Hive.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace Hive.Infrastructure.Security;

public sealed class PasswordHasher : IPasswordHasher
{
    private readonly PasswordHasher<User> _passwordHasher = new();

    public string HashPassword(string password)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);

        var user = new User();
        return _passwordHasher.HashPassword(user, password);
    }

    public bool VerifyPassword(string password, string passwordHash)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(password);
        ArgumentException.ThrowIfNullOrWhiteSpace(passwordHash);

        var user = new User();
        var result = _passwordHasher.VerifyHashedPassword(
            user,
            passwordHash,
            password);

        return result != PasswordVerificationResult.Failed;
    }
}
