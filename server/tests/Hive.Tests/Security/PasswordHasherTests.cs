using Hive.Infrastructure.Security;

namespace Hive.Tests.Security;

public sealed class PasswordHasherTests
{
    private readonly PasswordHasher _passwordHasher = new();

    [Fact]
    public void HashPassword_ReturnsHashDifferentFromOriginalPassword()
    {
        const string password = "CorrectPassword123";

        var hash = _passwordHasher.HashPassword(password);

        Assert.NotEqual(password, hash);
        Assert.NotEmpty(hash);
    }

    [Fact]
    public void VerifyPassword_CorrectPassword_ReturnsTrue()
    {
        const string password = "CorrectPassword123";
        var hash = _passwordHasher.HashPassword(password);

        var result = _passwordHasher.VerifyPassword(password, hash);

        Assert.True(result);
    }

    [Fact]
    public void VerifyPassword_IncorrectPassword_ReturnsFalse()
    {
        var hash = _passwordHasher.HashPassword("CorrectPassword123");

        var result = _passwordHasher.VerifyPassword("WrongPassword123", hash);

        Assert.False(result);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData(" ")]
    public void HashPassword_InvalidPassword_ThrowsArgumentException(string? password)
    {
        Assert.Throws<ArgumentException>(() => _passwordHasher.HashPassword(password!));
    }

    [Theory]
    [InlineData(null, "valid-hash")]
    [InlineData("", "valid-hash")]
    [InlineData(" ", "valid-hash")]
    [InlineData("password", null)]
    [InlineData("password", "")]
    [InlineData("password", " ")]
    public void VerifyPassword_InvalidInput_ThrowsArgumentException(
        string? password,
        string? passwordHash)
    {
        Assert.Throws<ArgumentException>(
            () => _passwordHasher.VerifyPassword(password!, passwordHash!));
    }
}
