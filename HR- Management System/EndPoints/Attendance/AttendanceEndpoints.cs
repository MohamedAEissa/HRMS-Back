using HR_Application.Features.Attendance.Commands.CreateAttendance;
using HR_Application.Features.Attendance.Commands.DeleteAttendance;
using HR_Application.Features.Attendance.Commands.UpdateAttendance;
using HR_Application.Features.Attendance.DTOs;
using HR_Application.Features.Attendance.Queries.GetAttendances;
using MediatR;

namespace HR__Management_System.EndPoints.Attendance
{
    public static class AttendanceEndpoints
    {
        public static void MapAttendanceEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/attendances")
                .WithTags("Attendance Management")
                .RequireAuthorization(policy => policy.RequireRole("Admin", "HR")); ;


            group.MapGet("/", async ([AsParameters] AttendanceFilterDto filter, ISender mediator) =>
            {
                var query = new GetAttendancesQuery(filter);
                var result = await mediator.Send(query);
                return Results.Ok(new
                {
                    Success = true,
               
                    Data = result
                });
            });

       
            group.MapPost("/", async (CreateAttendanceDto dto, ISender mediator) =>
            {
                var id = await mediator.Send(new CreateAttendanceCommand(dto));
                return Results.Created($"/api/attendances/{id}", new { Id = id, Message = "Attendance recorded successfully." });
            });

            
            group.MapPut("/{id:guid}", async (Guid id, UpdateAttendanceDto dto, ISender mediator) =>
            {
                var result = await mediator.Send(new UpdateAttendanceCommand(id, dto));
                return result ? Results.Ok(new { Message = "Attendance updated successfully." }) : Results.NotFound();
            });

            
            group.MapDelete("/{id:guid}", async (Guid id, ISender mediator) =>
            {
                var result = await mediator.Send(new DeleteAttendanceCommand(id));
                return result ? Results.Ok(new { Message = "Attendance deleted successfully." }) : Results.NotFound();
            });
        }
    }
}
