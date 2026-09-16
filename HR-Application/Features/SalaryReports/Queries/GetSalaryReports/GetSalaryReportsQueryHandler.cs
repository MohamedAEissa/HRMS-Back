using HR_Application.Features.SalaryReports.DTOs;
using HR_Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.SalaryReports.Queries.GetSalaryReports
{
    public class GetSalaryReportsQueryHandler : IRequestHandler<GetSalaryReportsQuery, List<SalaryReporResponsetDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetSalaryReportsQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<SalaryReporResponsetDto>> Handle(GetSalaryReportsQuery request, CancellationToken cancellationToken)
        {
            var query = _context.SalaryReports
                 .Include(s => s.Employee)
                 .ThenInclude(e => e.Department)
                 .AsQueryable();

            if (!string.IsNullOrWhiteSpace(request.Dto.EmployeeName))
            {
                query = query.Where(a => a.Employee.FullName != null &&
                                         a.Employee.FullName.ToLower().Contains(request.Dto.EmployeeName.ToLower()));
            }

            if (!string.IsNullOrWhiteSpace(request.Dto.DepartmentName))
            {
                query = query.Where(a => a.Employee.Department != null &&
                                         a.Employee.Department.Name.ToLower().Contains(request.Dto.DepartmentName.ToLower()));
            }

            if (request.Dto.EmployeeId.HasValue)
            {
                query = query.Where(s => s.EmployeeId == request.Dto.EmployeeId.Value);
            }

            if (request.Dto.DepartmentId.HasValue)
            {
                query = query.Where(s => s.Employee.DepartmentId == request.Dto.DepartmentId.Value);
            }

            if (request.Dto.Month.HasValue)
            {
                query = query.Where(s => s.Month == request.Dto.Month.Value);
            }

            if (request.Dto.Year.HasValue)
            {
                query = query.Where(s => s.Year == request.Dto.Year.Value);
            }


            var reports = await query
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

            return reports;
        }
    }
}
