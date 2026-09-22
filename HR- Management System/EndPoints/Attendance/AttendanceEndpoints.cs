using HR_Application.Features.Attendance.Commands.CreateAttendance;
using HR_Application.Features.Attendance.Commands.DeleteAttendance;
using HR_Application.Features.Attendance.Commands.ImportAttendanceFromExcel;
using HR_Application.Features.Attendance.Commands.UpdateAttendance;
using HR_Application.Features.Attendance.DTOs;
using HR_Application.Features.Attendance.DTOs.ExcelDto;
using HR_Application.Features.Attendance.Queries.GetAttendances;
using HR_Application.Features.Attendance.Queries.GetMyAttendance;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Threading;

namespace HR__Management_System.EndPoints.Attendance
{
    public static class AttendanceEndpoints
    {
        public static void MapAttendanceEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/attendances")
                .WithTags("Attendance Management")
                .RequireAuthorization();

            // GET: api/attendances/me 
            group.MapGet("/me", async ([AsParameters] AttendanceFilterDto filter, IMediator mediator, CancellationToken cancellationToken) =>
            {
                var result = await mediator.Send(new GetMyAttendanceQuery(filter), cancellationToken);
                return Results.Ok(new { Success = true, Data = result });
            })
            .RequireAuthorization()
            .Produces<List<AttendanceDto>>(StatusCodes.Status200OK);

            // GET: api/attendances
            group.MapGet("/", async ([AsParameters] AttendanceFilterDto filter, ISender mediator) =>
            {
                var query = new GetAttendancesQuery(filter);
                var result = await mediator.Send(query);
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .Produces<List<AttendanceDto>>(StatusCodes.Status200OK);

            // POST: api/attendances
            group.MapPost("/", async (CreateAttendanceDto dto, ISender mediator) =>
            {
                var id = await mediator.Send(new CreateAttendanceCommand(dto));
                return Results.Created($"/api/attendances/{id}", new { Id = id, Message = "Attendance recorded successfully." });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));

            // POST: api/attendances/import-excel
            group.MapPost("/import-excel", async (IFormFile file, ISender mediator, CancellationToken cancellationToken) =>
            {
                var dto = new ImportAttendanceExcelDto { File = file };
                var command = new ImportAttendanceExcelCommand(dto);
                var result = await mediator.Send(command, cancellationToken);

                return Results.Ok(new { Success = true, Data = result });
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"))
            .DisableAntiforgery()
            .Accepts<IFormFile>("multipart/form-data")
            .Produces<ImportAttendanceResultDto>(StatusCodes.Status200OK);

            // PUT: api/attendances/{id}
            group.MapPut("/{id:guid}", async (Guid id, UpdateAttendanceDto dto, ISender mediator) =>
            {
                var result = await mediator.Send(new UpdateAttendanceCommand(id, dto));
                return result ? Results.Ok(new { Message = "Attendance updated successfully." }) : Results.NotFound();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));

            // DELETE: api/attendances/{id}
            group.MapDelete("/{id:guid}", async (Guid id, ISender mediator) =>
            {
                var result = await mediator.Send(new DeleteAttendanceCommand(id));
                return result ? Results.Ok(new { Message = "Attendance deleted successfully." }) : Results.NotFound();
            })
            .RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));
        }
    }
}