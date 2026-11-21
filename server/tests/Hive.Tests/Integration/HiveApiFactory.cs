using System.Collections.Concurrent;
using Hive.Application.Abstractions.Persistence;
using Hive.Application.Abstractions.Security;
using Hive.Application.Common.Exceptions;
using Hive.Domain.Entities;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Hive.Tests.Integration;

public sealed class HiveApiFactory : WebApplicationFactory<Program>
{
    public FakeUserRepository Users { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");

        builder.ConfigureTestServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IPasswordHasher>();

            services.AddSingleton<IUserRepository>(Users);
            services.AddSingleton<IPasswordHasher, TestPasswordHasher>();
        });
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
    }
}

public sealed class FakeUserRepository : IUserRepository
{
    private readonly ConcurrentDictionary<string, User> _users =
        new(StringComparer.OrdinalIgnoreCase);

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        _users.TryGetValue(email, out var user);
        return Task.FromResult(user);
    }

    public Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        if (!_users.TryAdd(user.Email, user))
            throw new EmailAlreadyExistsException();

        return Task.CompletedTask;
    }
}

public sealed class TestPasswordHasher : IPasswordHasher
{
    public string HashPassword(string password) => $"test-hash:{password}";

    public bool VerifyPassword(string password, string passwordHash) =>
        passwordHash == HashPassword(password);
}
