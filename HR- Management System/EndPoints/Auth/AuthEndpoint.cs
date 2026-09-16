using HR_Application.Features.Accounts.Commands.DeleteAccount;
using HR_Application.Features.Accounts.Queries.GetAllAccounts;
using HR_Application.Features.Auth.Commands.CreateAccount;
using HR_Application.Features.Auth.Commands.LoginAccount;
using HR_Application.Features.Auth.Commands.Logout;
using HR_Application.Features.Auth.Commands.RefreshToken;
using HR_Application.Features.Auth.Commands.UpdateAccount;
using HR_Application.Features.Auth.DTOs;
using MediatR;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace HR__Management_System.EndPoints.Auth
{
    public static class AuthEndpoint
    {
        public static void MapAuthEndpoint(this IEndpointRouteBuilder endpoints)
        {
            var authGroup = endpoints.MapGroup("api/auth")
                .WithTags("Auth");

            // 1. Create Account
            authGroup.MapPost("/create-account", async (CreateAccountDto dto, IMediator mediator) =>
            {
                var command = new CreateAccountCommand(dto);
                var result = await mediator.Send(command);

                if (result)
                {
                    return Results.Ok(new { Success = true, Message = "Account created and role assigned successfully." });
                }

                return Results.BadRequest("Failed to create account.");
            })
            .WithName("CreateAccount")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // 2. Login
            authGroup.MapPost("/login", async (LoginDto dto, IMediator mediator) =>
            {
                var command = new LoginAccountCommand(dto);
                var result = await mediator.Send(command);

                return Results.Ok(new
                {
                    Success = true,
                    Message = "Logged in successfully.",
                    Data = result
                });
            })
            .WithName("Login")
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // 3. Refresh Token
            authGroup.MapPost("/refresh-token", async (RefreshTokenDto model, ISender mediator) =>
            {
                var result = await mediator.Send(new RefreshTokenCommand(model));
                return Results.Ok(result);
            })
            .WithName("RefreshToken")
            .Produces<AuthResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // 4. Logout
            authGroup.MapPost("/logout", async (ISender mediator) =>
            {
                var result = await mediator.Send(new LogoutCommand());
                return result ? Results.Ok("Logged out") : Results.BadRequest();
            })
            .WithName("Logout")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            // 5. Get All Accounts
            authGroup.MapGet("/accounts", async (ISender mediator) =>
            {
                var result = await mediator.Send(new GetAllAccountsQuery());
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .WithName("GetAllAccounts")
            .RequireAuthorization()
            .Produces<List<UserAccountDto>>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status401Unauthorized);

            // 6. Delete Account
            authGroup.MapDelete("/accounts/{id}", async (string id, ISender mediator) =>
            {
                var result = await mediator.Send(new DeleteAccountCommand(id));
                return result
                    ? Results.Ok(new { Success = true, Message = "Account deleted successfully." })
                    : Results.BadRequest("Failed to delete account.");
            })
            .WithName("DeleteAccount")
            .RequireAuthorization()
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized);


            authGroup.MapPut("/accounts/{id:guid}", async (Guid id, UpdateUserAccountDto dto, ISender mediator) =>
            {
                var command = new UpdateUserAccountCommand(id, dto);
                var result = await mediator.Send(command);

                return Results.Ok(new
                {
                    Success = true,
                    Message = "Account updated successfully.",
                    Data = result
                });
            })
            .WithName("UpdateAccount")
            .RequireAuthorization()
            .Produces<UserAccountDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status401Unauthorized)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}