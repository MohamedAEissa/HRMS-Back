using HR__Management_System.EndPoints.Attendance;
using HR__Management_System.EndPoints.Auth;
using HR__Management_System.EndPoints.DepartmentEndpoints;
using HR__Management_System.EndPoints.Employees;
using HR__Management_System.EndPoints.GeneralSettings;
using HR__Management_System.EndPoints.OfficialHoliday;
using HR__Management_System.Extentions;
using HR_API.Endpoints;
using HR_Application;
using HR_Domain.Entities; 
using HR_Infrastructure;
using HR_Infrastructure.Context; 
using HR_Management_System.Endpoints;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services
    .AddInfrastructureServices(builder.Configuration)
    .AddApplicationServices()
    .AddApiServices(builder.Configuration, builder.Host);

builder.Services.AddControllers();

var app = builder.Build();


using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
        var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();

        // 1. ????? ?????? ??? ???????? ???????? ?? ?? ??????
        await context.Database.MigrateAsync();

        
        string[] roleNames = { "Admin", "HR", "Employee" };
        foreach (var roleName in roleNames)
        {
            var roleExist = await roleManager.RoleExistsAsync(roleName);
            if (!roleExist)
            {
                await roleManager.CreateAsync(new ApplicationRole { Name = roleName });
            }
        }

        // 3. ????? ???? Admin ??????? ???????
        var adminEmail = "admin@hrms.com";
        var adminUser = await userManager.FindByEmailAsync(adminEmail);

        if (adminUser == null)
        {
            var newAdmin = new ApplicationUser
            {
                UserName = adminEmail,
                Email = adminEmail,
                EmailConfirmed = true
            };

            // ???????? ????????? ??? Admin
            var createPowerUser = await userManager.CreateAsync(newAdmin, "Admin@123456");
            if (createPowerUser.Succeeded)
            {
                await userManager.AddToRoleAsync(newAdmin, "Admin");
            }
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while migrating or seeding the database.");
    }
}
// ==========================================

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