using HR_Application.Features.SalaryReports.Commands.CreateSalaryReport;
using HR_Application.Features.SalaryReports.Commands.DeleteSalaryReport;
using HR_Application.Features.SalaryReports.Commands.UpdateSalaryReport;
using HR_Application.Features.SalaryReports.DTOs;
using HR_Application.Features.SalaryReports.Queries.GetMySalaryReports;
using HR_Application.Features.SalaryReports.Queries.GetSalaryReportById;
using HR_Application.Features.SalaryReports.Queries.GetSalaryReports;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace HR_Management_System.Endpoints
{
    public static class SalaryReportEndpoints
    {
        public static void MapSalaryReportEndpoints(this IEndpointRouteBuilder endpoints)
        {
            var group = endpoints.MapGroup("api/salary-reports")
                .WithTags("Salary Reports Management")
                .RequireAuthorization();


            group.MapGet("/me", async ([AsParameters] salaryReportFilterDto filter, ISender mediator) =>
            {
                var query = new GetMySalaryReportsQuery(filter);
                var result = await mediator.Send(query);
                return Results.Ok(new { Success = true, Data = result });
            })
            .RequireAuthorization()
            .Produces<List<SalaryReporResponsetDto>>(StatusCodes.Status200OK);

            group.MapGet("/", async ([AsParameters] salaryReportFilterDto filter , ISender mediator) =>
            {
                var query = new GetSalaryReportsQuery(filter);
                var result = await mediator.Send(query);
                return Results.Ok(new
                {
                    Success = true,
                    Data = result
                });
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));


            group.MapGet("/{id:guid}", async (Guid id, ISender mediator) =>
            {
                var query = new GetSalaryReportByIdQuery(id);
                var result = await mediator.Send(query);
                return result != null ? Results.Ok(new
                {
                    Success = true,
                    Data = result
                }) : Results.NotFound();
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));


            group.MapPost("/", async (RequestSalaryReportDto dto, ISender mediator) =>
            {
                var result = await mediator.Send(new CreateSalaryReportCommand(dto));
                return Results.Created($"/api/salary-reports/{result.Id}", new { Data = result, Message = "Salary report generated successfully." , Success = true });
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));


            group.MapPut("/{id:guid}", async (Guid id, RequestSalaryReportDto dto, ISender mediator) =>
            {
                var result = await mediator.Send(new UpdateSalaryReportCommand(id, dto));
                return Results.Ok(new { Data = result, Message = "Salary report updated successfully.", Success = true });
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));


            group.MapDelete("/{id:guid}", async (Guid id, ISender mediator) =>
            {
                var result = await mediator.Send(new DeleteSalaryReportCommand(id));
                return result ? Results.Ok(new { Message = "Salary report deleted successfully." , Success = true }) : Results.NotFound();
            }).RequireAuthorization(policy => policy.RequireRole("Admin", "HR"));
        }
    }
}