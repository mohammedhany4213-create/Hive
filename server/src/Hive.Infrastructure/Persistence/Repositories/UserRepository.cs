using Hive.Application.Abstractions.Persistence;
using Hive.Application.Features.Authentication.Register;
using Hive.Domain.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace Hive.Infrastructure.Persistence.Repositories;

public sealed class UserRepository : IUserRepository
{
    private const string EmailUniqueIndexName = "IX_Users_Email";

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

        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsEmailUniqueViolation(exception))
        {
            throw new EmailAlreadyExistsException();
        }
    }

    private static bool IsEmailUniqueViolation(DbUpdateException exception)
    {
        return exception.InnerException is SqlException sqlException &&
            sqlException.Errors
                .Cast<SqlError>()
                .Any(error =>
                    error.Number is 2601 or 2627 &&
                    error.Message.Contains(
                        EmailUniqueIndexName,
                        StringComparison.OrdinalIgnoreCase));
    }
}
