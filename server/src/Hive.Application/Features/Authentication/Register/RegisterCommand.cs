namespace Hive.Application.Features.Authentication.Register;

public sealed record RegisterCommand(
    string FullName,
    string Email,
    string Password);
