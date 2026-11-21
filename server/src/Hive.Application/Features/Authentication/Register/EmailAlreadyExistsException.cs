namespace Hive.Application.Features.Authentication.Register;

public sealed class EmailAlreadyExistsException : Exception
{
    public EmailAlreadyExistsException()
        : base("An account with this email address already exists.")
    {
    }
}
