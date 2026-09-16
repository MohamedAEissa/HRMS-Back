 using HR_Application.Features.Employees.Commands.CreateEmployee;
using HR_Application.Features.Employees.Commands.DeleteEmployee;
using HR_Application.Features.Employees.Commands.UpdateEmployee;
using HR_Application.Features.Employees.DTOs;
using HR_Application.Features.Employees.Queries;
using HR_Application.Features.Employees.Queries.GetMyProfile;
using MediatR;

namespace HR__Management_System.EndPoints.Employees
{
    public static class EmployeeEndpoints
    {
        public static void MapEmployeeEndpoints(this IEndpointRouteBuilder app)
        {
      
            var group = app.MapGroup("api/employees")
                           .WithTags("Employees")
                           .RequireAuthorization();

            group.MapGet("/me", async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetMyProfileQuery(), cancellationToken);

                if (result == null)
                {
                    return Results.NotFound(new
                    {
                        Success = false,
                        Message = "Employee profile not found for the logged-in user."
                    });
                }

                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .Produces<EmployeeResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound)
            .Produces(StatusCodes.Status401Unauthorized);

        

            // 1. Get All Employees
            group.MapGet("/", async (IMediator mediator, CancellationToken cancellationToken) =>
            {
                var query = new GetAllEmployeesQuery();
                var result = await mediator.Send(query, cancellationToken);
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .Produces<List<EmployeeResponseDto>>(StatusCodes.Status200OK);

            // 2. Get Employee By Id
            group.MapGet("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetEmployeeByIdQuery(id), cancellationToken);
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .Produces<EmployeeResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status404NotFound);

            // 3. Create Employee
            group.MapPost("/", async (CreateEmployeeDto dto, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new CreateEmployeeCommand(dto), cancellationToken);
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .Produces<EmployeeResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest);

            // 4. Update Employee
            group.MapPut("/{id:guid}", async (Guid id, UpdateEmployeeDto dto, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new UpdateEmployeeCommand(id, dto), cancellationToken);
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .Produces<EmployeeResponseDto>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .Produces(StatusCodes.Status404NotFound);

            // 5. Delete Employee
            group.MapDelete("/{id:guid}", async (Guid id, IMediator mediator, CancellationToken cancellationToken) =>
            {
                await mediator.Send(new DeleteEmployeeCommand(id), cancellationToken);
                return Results.Ok(new { message = "Employee deleted successfully.", Success = true });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .Produces(StatusCodes.Status204NoContent)
            .Produces(StatusCodes.Status404NotFound);
        }
    }
}