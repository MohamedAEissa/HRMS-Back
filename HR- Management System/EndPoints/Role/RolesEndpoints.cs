using HR_Application.Features.Roles.Commands.AddRole;
using HR_Application.Features.Roles.Commands.DeleteRole;
using HR_Application.Features.Roles.Commands.UpdateRole;
using HR_Application.Features.Roles.DTOs;
using HR_Application.Features.Roles.Queries.GetAllRoles;

using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Threading.Tasks;

namespace HR_API.Endpoints
{
    public static class RolesEndpoints
    {
        public static void MapRoleEndpoints(this IEndpointRouteBuilder app)
        {
            var group = app.MapGroup("/api/roles")
                           .WithTags("Roles Management");

           
            group.MapGet("/", async (IMediator mediator) =>
            {
                var result = await mediator.Send(new GetAllRolesQuery());
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .WithName("GetAllRoles")
            .Produces<List<RoleResponseDto>>(StatusCodes.Status200OK);

            // 3. Create Role
            group.MapPost("/", async (AddRoleDto dto, IMediator mediator) =>
            {
                var result = await mediator.Send(new AddRoleCommand(dto));
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .Produces<RoleResponseDto>(StatusCodes.Status201Created)
            .Produces(StatusCodes.Status400BadRequest);

            // 4. Update Role
            group.MapPut("/{id:guid}", async (Guid id, UpdateRoleDto dto, IMediator mediator) =>
            {
                dto.Id = id;
                var result = await mediator.Send(new UpdateRoleCommand(dto));
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .WithName("UpdateRole")
            .Produces<RoleResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // 5. Delete Role
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator) =>
            {
                await mediator.Send(new DeleteRoleCommand(id));
                return Results.Ok(new { message = "Employee deleted successfully.", Success = true });
            })
            .WithName("DeleteRole")
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}