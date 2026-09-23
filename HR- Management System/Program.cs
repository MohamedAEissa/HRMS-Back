using HR__Management_System.EndPoints.Attendance;
using HR__Management_System.EndPoints.Auth;
using HR__Management_System.EndPoints.DepartmentEndpoints;
using HR__Management_System.EndPoints.Employees;
using HR__Management_System.EndPoints.GeneralSettings;
using HR__Management_System.EndPoints.OfficialHoliday;
using HR__Management_System.Extentions;
using HR_API.Endpoints;
using HR_Application;
using HR_Infrastructure;
using HR_Management_System.Endpoints;
using Microsoft.AspNetCore.Identity;



var builder = WebApplication.CreateBuilder(args);


builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices()
    .AddApiServices(builder.Configuration, builder.Host);

builder.Services.AddControllers();

var app = builder.Build();


app.UseApiMiddelwares();
app.MapAuthEndpoint();
app.MapDepartmentEndpoints();
app.MapEmployeeEndpoints();
app.MapGeneralSettingsEndpoint();
app.MapOfficialHolidayEndpoint();
app.MapAttendanceEndpoints();
app.MapSalaryReportEndpoints();
app.MapRoleEndpoints();
app.Run();
