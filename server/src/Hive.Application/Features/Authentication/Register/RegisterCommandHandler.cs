using System.Net.Mail;
using Hive.Application.Abstractions.Persistence;
using Hive.Application.Abstractions.Security;
using Hive.Domain.Entities;

namespace Hive.Application.Features.Authentication.Register;

public sealed class RegisterCommandHandler
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
    }

    public async Task<RegisterResult> HandleAsync(
        RegisterCommand command,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(command);

        var fullName = command.FullName?.Trim();
        var email = command.Email?.Trim().ToLowerInvariant();
        var password = command.Password;

        if (string.IsNullOrWhiteSpace(fullName))
            throw new ArgumentException("Full name is required.", nameof(command));

        if (fullName.Length > 150)
            throw new ArgumentException("Full name cannot exceed 150 characters.", nameof(command));

        if (string.IsNullOrWhiteSpace(email) || email.Length > 256 ||
            !MailAddress.TryCreate(email, out var parsedEmail) ||
            !string.Equals(parsedEmail.Address, email, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("A valid email address is required.", nameof(command));
        }

        if (string.IsNullOrWhiteSpace(password) || password.Length < 8)
            throw new ArgumentException("Password must be at least 8 characters long.", nameof(command));

        var existingUser = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (existingUser is not null)
            throw new EmailAlreadyExistsException();

        var user = new User
        {
            UserId = Guid.NewGuid(),
            FullName = fullName,
            Email = email,
            PasswordHash = _passwordHasher.HashPassword(password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.AddAsync(user, cancellationToken);

        return new RegisterResult(
            user.UserId,
            user.FullName,
            user.Email,
            user.CreatedAt);
    }
}
