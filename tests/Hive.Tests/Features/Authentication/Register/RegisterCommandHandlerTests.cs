using Hive.Application.Abstractions.Persistence;
using Hive.Application.Abstractions.Security;
using Hive.Application.Features.Authentication.Register;
using Hive.Domain.Entities;

namespace Hive.Tests.Features.Authentication.Register;

public sealed class RegisterCommandHandlerTests
{
    [Fact]
    public async Task HandleAsync_ValidCommand_CreatesUserAndReturnsResult()
    {
        var repository = new FakeUserRepository();
        var handler = new RegisterCommandHandler(repository, new FakePasswordHasher());

        var result = await handler.HandleAsync(
            new RegisterCommand("  Mohamed Hany  ", "  MOHAMED@example.com ", "Password123"));

        var savedUser = Assert.Single(repository.AddedUsers);
        Assert.Equal("Mohamed Hany", savedUser.FullName);
        Assert.Equal("mohamed@example.com", savedUser.Email);
        Assert.Equal("hashed:Password123", savedUser.PasswordHash);
        Assert.Equal(savedUser.UserId, result.UserId);
        Assert.Equal(savedUser.Email, result.Email);
        Assert.DoesNotContain("PasswordHash", result.ToString());
    }

    [Fact]
    public async Task HandleAsync_ExistingEmail_ThrowsEmailAlreadyExistsException()
    {
        var repository = new FakeUserRepository
        {
            ExistingUser = new User { Email = "mohamed@example.com" }
        };
        var handler = new RegisterCommandHandler(repository, new FakePasswordHasher());

        await Assert.ThrowsAsync<EmailAlreadyExistsException>(() =>
            handler.HandleAsync(
                new RegisterCommand("Mohamed Hany", "MOHAMED@example.com", "Password123")));

        Assert.Empty(repository.AddedUsers);
    }

    [Theory]
    [InlineData("", "mohamed@example.com", "Password123")]
    [InlineData("Mohamed", "not-an-email", "Password123")]
    [InlineData("Mohamed", "mohamed@example.com", "short")]
    public async Task HandleAsync_InvalidInput_ThrowsArgumentException(
        string fullName,
        string email,
        string password)
    {
        var handler = new RegisterCommandHandler(
            new FakeUserRepository(),
            new FakePasswordHasher());

        await Assert.ThrowsAsync<ArgumentException>(() =>
            handler.HandleAsync(new RegisterCommand(fullName, email, password)));
    }

    private sealed class FakeUserRepository : IUserRepository
    {
        public User? ExistingUser { get; init; }
        public List<User> AddedUsers { get; } = [];

        public Task<User?> GetByEmailAsync(
            string email,
            CancellationToken cancellationToken = default)
        {
            return Task.FromResult(
                ExistingUser?.Email.Equals(email, StringComparison.OrdinalIgnoreCase) == true
                    ? ExistingUser
                    : null);
        }

        public Task AddAsync(
            User user,
            CancellationToken cancellationToken = default)
        {
            AddedUsers.Add(user);
            return Task.CompletedTask;
        }
    }

    private sealed class FakePasswordHasher : IPasswordHasher
    {
        public string HashPassword(string password) => $"hashed:{password}";

        public bool VerifyPassword(string password, string passwordHash) =>
            passwordHash == $"hashed:{password}";
    }
}
