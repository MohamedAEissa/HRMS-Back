using HR_Application.Features.Employees.DTOs;
using HR_Application.Interfaces.Persistence;
using HR_Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR_Application.Features.Employees.Queries.GetMyProfile
{
    public class GetMyProfileQueryHandler : IRequestHandler<GetMyProfileQuery, EmployeeResponseDto?>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyProfileQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<EmployeeResponseDto?> Handle(GetMyProfileQuery request, CancellationToken cancellationToken)
        {
            var userEmail = _currentUserService.UserEmail;

            if (string.IsNullOrEmpty(userEmail))
            {
                return null;
            }

            
            var employeeDto = await (from emp in _context.Employees.AsNoTracking()
                                     where emp.Email == userEmail
                                     join usr in _context.ApplicationUser.AsNoTracking() on emp.Email equals usr.Email into userGroup
                                     from usr in userGroup.DefaultIfEmpty()
                                     join role in _context.ApplicationRoles.AsNoTracking() on usr.RoleId equals role.Id into roleGroup
                                     from role in roleGroup.DefaultIfEmpty()

                                     select new EmployeeResponseDto
                                     {
                                         Id = emp.Id,
                                         FullName = string.IsNullOrEmpty(emp.FullName) ? (usr != null ? usr.FullName : string.Empty) : emp.FullName,
                                         Email = emp.Email,
                                         Phone = emp.Phone,
                                         Salary = emp.Salary,
                                         DepartmentId = emp.DepartmentId,
                                         DepartmentName = emp.Department != null ? emp.Department.Name : string.Empty,

                                         RoleName = role != null ? role.Name : "Employee",
                                         IsActive = usr == null || usr.IsActive,
                                         CreatedAt = usr != null ? usr.CreatedAt : DateTime.UtcNow
                                     })
                                     .FirstOrDefaultAsync(cancellationToken);

            return employeeDto;
        }
    }
}