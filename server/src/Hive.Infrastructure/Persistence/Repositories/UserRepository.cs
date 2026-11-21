using Hive.Application.Abstractions.Persistence;
using Hive.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace Hive.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private readonly HiveDbContext _dbContext;

    public UserRepository(HiveDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public Task<User?> GetByEmailAsync(
        string email,
        CancellationToken cancellationToken = default)
    {
        return _dbContext.Users
            .SingleOrDefaultAsync(
                user => user.Email == email,
                cancellationToken);
    }

    public async Task AddAsync(
        User user,
        CancellationToken cancellationToken = default)
    {
        await _dbContext.Users.AddAsync(user, cancellationToken);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}
