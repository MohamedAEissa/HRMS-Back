using HR_Application.Features.SalaryReports.DTOs;
using HR_Application.Interfaces.Persistence;
using HR_Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR_Application.Features.SalaryReports.Queries.GetMySalaryReports
{
    public record GetMySalaryReportsQuery(salaryReportFilterDto Dto) : IRequest<List<SalaryReporResponsetDto>>;

    public class GetMySalaryReportsQueryHandler : IRequestHandler<GetMySalaryReportsQuery, List<SalaryReporResponsetDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMySalaryReportsQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<SalaryReporResponsetDto>> Handle(GetMySalaryReportsQuery request, CancellationToken cancellationToken)
        {
            var userEmail = _currentUserService.UserEmail;
            if (string.IsNullOrEmpty(userEmail)) return new();

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == userEmail, cancellationToken);

            if (employee == null) return new();

          
            var query = _context.SalaryReports
                .AsNoTracking()
                .Where(s => s.EmployeeId == employee.Id);

         
            if (request.Dto != null)
            {
                if (request.Dto.Month.HasValue)
                {
                    query = query.Where(s => s.Month == request.Dto.Month.Value);
                }

                if (request.Dto.Year.HasValue)
                {
                    query = query.Where(s => s.Year == request.Dto.Year.Value);
                }
            }

           
            var salaryReports = await query
                .OrderByDescending(s => s.Year)
                .ThenByDescending(s => s.Month)
                .Select(s => new SalaryReporResponsetDto
                {
                    Id = s.Id,
                    EmployeeId = s.EmployeeId,
                    EmployeeName = s.Employee.FullName,
                    DepartmentName = s.Employee.Department != null ? s.Employee.Department.Name : "No Department",
                    Month = s.Month,
                    Year = s.Year,
                    BasicSalary = s.BasicSalary,
                    AttendanceDays = s.AttendanceDays,
                    AbsenceDays = s.AbsenceDays,
                    TotalOvertimeHours = s.TotalOvertimeHours,
                    TotalDeductionHours = s.TotalDeductionHours,
                    TotalOvertimeAmount = s.TotalOvertimeAmount,
                    TotalDeductionAmount = s.TotalDeductionAmount,
                    NetSalary = s.NetSalary
                })
                .ToListAsync(cancellationToken);

            return salaryReports;
        }
    }
}