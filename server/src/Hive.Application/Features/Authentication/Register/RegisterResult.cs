namespace Hive.Application.Features.Authentication.Register;

public sealed record RegisterResult(
    Guid UserId,
    string FullName,
    string Email,
    DateTime CreatedAt);
