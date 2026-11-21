using Hive.Application.Common.Exceptions;
using Hive.Application.Features.Authentication.Register;
using Microsoft.AspNetCore.Mvc;

namespace Hive.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class AuthController : ControllerBase
{
    private readonly RegisterCommandHandler _registerHandler;

    public AuthController(RegisterCommandHandler registerHandler)
    {
        _registerHandler = registerHandler;
    }

    [HttpPost("register")]
    [ProducesResponseType<RegisterResult>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<RegisterResult>> Register(
        [FromBody] RegisterRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await _registerHandler.HandleAsync(
                new RegisterCommand(request.FullName, request.Email, request.Password),
                cancellationToken);

            return StatusCode(StatusCodes.Status201Created, result);
        }
        catch (EmailAlreadyExistsException exception)
        {
            return Conflict(new ProblemDetails
            {
                Status = StatusCodes.Status409Conflict,
                Title = "Email already registered",
                Detail = exception.Message
            });
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Invalid registration data",
                Detail = exception.Message
            });
        }
    }
}

public sealed record RegisterRequest(
    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.StringLength(150, MinimumLength = 2)]
    string FullName,

    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.EmailAddress]
    [property: System.ComponentModel.DataAnnotations.StringLength(256)]
    string Email,

    [property: System.ComponentModel.DataAnnotations.Required]
    [property: System.ComponentModel.DataAnnotations.StringLength(128, MinimumLength = 8)]
    string Password);
