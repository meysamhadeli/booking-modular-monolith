using Griffin.Core.Constants;
using Duende.IdentityServer.EntityFramework.Entities;

namespace Identity.Identity.Features.RegisteringNewUser.V1;

using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Ardalis.GuardClauses;
using Griffin.Core.EventBus.Messages;
using Griffin.Core;
using Griffin.Core.CQRS;
using Griffin.Web;
using Exceptions;
using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Models;

public record RegisterNewUser(string FirstName, string LastName, string Username, string Email,
    string Password, string ConfirmPassword, string PassportNumber) : ICommand<RegisterNewUserResult>;

public record RegisterNewUserResult(Guid Id, string FirstName, string LastName, string Username, string PassportNumber);

public record RegisterNewUserRequestDto(string FirstName, string LastName, string Username, string Email,
    string Password, string ConfirmPassword, string PassportNumber);

public record RegisterNewUserResponseDto(Guid Id, string FirstName, string LastName, string Username,
    string PassportNumber);

public class RegisterNewUserEndpoint : IMinimalEndpoint
{
    public IEndpointRouteBuilder MapEndpoint(IEndpointRouteBuilder builder)
    {
        builder.MapPost($"{EndpointConfig.BaseApiPath}/identity/register-user", async (
                RegisterNewUserRequestDto request, IMediator mediator, IMapper mapper,
                CancellationToken cancellationToken) =>
            {
                var command = mapper.Map<RegisterNewUser>(request);

                var result = await mediator.Send(command, cancellationToken);

                var response = result.Adapt<RegisterNewUserResponseDto>();

                return Results.Ok(response);
            })
            .RequireAuthorization(nameof(ApiScope))
            .WithName("RegisterUser")
            .WithApiVersionSet(builder.NewApiVersionSet("Identity").Build())
            .Produces<RegisterNewUserResponseDto>()
            .ProducesProblem(StatusCodes.Status400BadRequest)
            .WithSummary("Register User")
            .WithDescription("Register User")
            .WithOpenApi()
            .HasApiVersion(1.0);

        return builder;
    }
}

public class RegisterNewUserValidator : AbstractValidator<RegisterNewUser>
{
    public RegisterNewUserValidator()
    {
        RuleFor(x => x.Password).NotEmpty().WithMessage("Please enter the password");
        RuleFor(x => x.ConfirmPassword).NotEmpty().WithMessage("Please enter the confirmation password");

        RuleFor(x => x).Custom((x, context) =>
        {
            if (x.Password != x.ConfirmPassword)
            {
                context.AddFailure(nameof(x.Password), "Passwords should match");
            }
        });

        RuleFor(x => x.Username).NotEmpty().WithMessage("Please enter the username");

        RuleFor(x => x.FirstName).NotEmpty().WithMessage("Please enter the first name")
            .MaximumLength(50).WithMessage("First name must be at most 50 characters");
        RuleFor(x => x.LastName).NotEmpty().WithMessage("Please enter the last name")
            .MaximumLength(50).WithMessage("Last name must be at most 50 characters");

        RuleFor(x => x.Email).NotEmpty().WithMessage("Please enter the last email")
            .EmailAddress().WithMessage("A valid email is required");

        RuleFor(x => x.PassportNumber).NotEmpty().WithMessage("Please enter the passport number")
            .MaximumLength(10).WithMessage("Passport number must be at most 10 characters");

        // `UserCreated` carries `FirstName + " " + LastName` as its `Name`, and in this modular
        // monolith that message is handled in-process by the Passenger module, which persists
        // the name into a `character varying(50)` column. Validate the combined length here so
        // over-long names fail fast with a 400 instead of surfacing as a Postgres error
        // (`22001: value too long for type character varying(50)`) inside another module.
        RuleFor(x => x).Custom((x, context) =>
        {
            var fullName = $"{x.FirstName} {x.LastName}".Trim();
            if (fullName.Length > 50)
            {
                context.AddFailure(nameof(x.FirstName),
                    "First name and last name combined must be at most 50 characters");
            }
        });
    }
}

internal class RegisterNewUserHandler : ICommandHandler<RegisterNewUser, RegisterNewUserResult>
{
    private readonly IEventDispatcher _eventDispatcher;
    private readonly UserManager<User> _userManager;

    public RegisterNewUserHandler(UserManager<User> userManager,
        IEventDispatcher eventDispatcher)
    {
        _userManager = userManager;
        _eventDispatcher = eventDispatcher;
    }

    public async Task<RegisterNewUserResult> Handle(RegisterNewUser request,
        CancellationToken cancellationToken)
    {
        Guard.Against.Null(request, nameof(request));

        var applicationUser = new User()
        {
            FirstName = request.FirstName,
            LastName = request.LastName,
            UserName = request.Username,
            Email = request.Email,
            PasswordHash = request.Password,
            PassPortNumber = request.PassportNumber
        };

        var identityResult = await _userManager.CreateAsync(applicationUser, request.Password);
        var roleResult = await _userManager.AddToRoleAsync(applicationUser, IdentityConstant.Role.User);

        if (identityResult.Succeeded == false)
        {
            throw new RegisterIdentityUserException(string.Join(',', identityResult.Errors.Select(e => e.Description)));
        }

        if (roleResult.Succeeded == false)
        {
            throw new RegisterIdentityUserException(string.Join(',', roleResult.Errors.Select(e => e.Description)));
        }

        await _eventDispatcher.SendAsync(new UserCreated(applicationUser.Id,
            applicationUser.FirstName + " " + applicationUser.LastName,
            applicationUser.PassPortNumber), cancellationToken: cancellationToken);

        return new RegisterNewUserResult(applicationUser.Id, applicationUser.FirstName, applicationUser.LastName,
            applicationUser.UserName, applicationUser.PassPortNumber);
    }
}