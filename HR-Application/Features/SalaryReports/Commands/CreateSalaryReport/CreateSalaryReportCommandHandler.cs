using HR_Application.Features.SalaryReports.DTOs;
using HR_Application.Interfaces.Persistence;
using HR_Domain.Entities;
using HR_Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.SalaryReports.Commands.CreateSalaryReport
{
    public class CreateSalaryReportCommandHandler : IRequestHandler<CreateSalaryReportCommand, SalaryReporResponsetDto>
    {
        private readonly IApplicationDbContext _context;

        public CreateSalaryReportCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<SalaryReporResponsetDto> Handle(CreateSalaryReportCommand request, CancellationToken cancellationToken)
        {
            var dto = request.Dto;

            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == dto.EmployeeId, cancellationToken);

            if (employee == null) throw new Exception("Employee not found.");

         
            var settings = await _context.GeneralSettings.FirstOrDefaultAsync(cancellationToken);
            decimal overtimeMultiplier = settings.OvertimeHourRate ;
            decimal deductionMultiplier = settings.DeductionHourRate ;

         
            var weeklyDaysOff = (settings?.WeeklyDaysOff)
                .Split(',', StringSplitOptions.RemoveEmptyEntries)
                .Select(d => Enum.TryParse<DayOfWeek>(d.Trim(), true, out var day) ? day : (DayOfWeek?)null)
                .Where(d => d.HasValue)
                .Select(d => d!.Value)
                .ToList();

            var officialHolidays = await _context.OfficialHolidays
                .Where(h => h.Date.Month == dto.Month && h.Date.Year == dto.Year)
                .Select(h => h.Date.Date)
                .ToListAsync(cancellationToken);

          
            var attendances = await _context.Attendances
                .Where(a => a.EmployeeId == dto.EmployeeId &&
                            a.Date.Month == dto.Month &&
                            a.Date.Year == dto.Year)
                .ToListAsync(cancellationToken);

            
            int attendanceDays = attendances.Count(a => a.Status == AttendanceStatus.Present || a.Status == AttendanceStatus.late);

            int absenceDays = attendances.Count(a => a.Status == AttendanceStatus.Absent &&
                                                    !officialHolidays.Contains(a.Date.Date) &&
                                                    !weeklyDaysOff.Contains(a.Date.DayOfWeek));

            decimal totalOvertimeHours = attendances.Sum(a => a.OvertimeHours);
            decimal totalDeductionHours = attendances.Sum(a => a.DeductionHours);

            decimal hourlyRate = employee.Salary > 0 ? (employee.Salary / 30m / 8m) : 0;
            decimal dailyRate = employee.Salary > 0 ? (employee.Salary / 30m) : 0;

            decimal totalOvertimeAmount = totalOvertimeHours * hourlyRate * overtimeMultiplier;
            decimal delayDeductionAmount = totalDeductionHours * hourlyRate * deductionMultiplier;
            decimal absenceDeductionAmount = absenceDays * dailyRate;
            decimal totalDeductionAmount = delayDeductionAmount + absenceDeductionAmount;

            decimal netSalary = employee.Salary + totalOvertimeAmount - totalDeductionAmount;

            // 5. Save Report
            var salaryReport = new SalaryReport
            {
                EmployeeId = dto.EmployeeId,
                Month = dto.Month,
                Year = dto.Year,
                BasicSalary = employee.Salary,
                AttendanceDays = attendanceDays,
                AbsenceDays = absenceDays,
                TotalOvertimeHours = totalOvertimeHours,
                TotalDeductionHours = totalDeductionHours,
                TotalOvertimeAmount = Math.Round(totalOvertimeAmount, 2),
                TotalDeductionAmount = Math.Round(totalDeductionAmount, 2),
                NetSalary = Math.Round(netSalary, 2)
            };

            _context.SalaryReports.Add(salaryReport);
            await _context.SaveChangesAsync(cancellationToken);

            return new SalaryReporResponsetDto
            {
                Id = salaryReport.Id,
                EmployeeId = employee.Id,
                EmployeeName = employee.FullName,
                DepartmentName = employee.Department?.Name ?? "No Department",
                Month = salaryReport.Month,
                Year = salaryReport.Year,
                BasicSalary = salaryReport.BasicSalary,
                AttendanceDays = salaryReport.AttendanceDays,
                AbsenceDays = salaryReport.AbsenceDays,
                TotalOvertimeHours = salaryReport.TotalOvertimeHours,
                TotalDeductionHours = salaryReport.TotalDeductionHours,
                TotalOvertimeAmount = salaryReport.TotalOvertimeAmount,
                TotalDeductionAmount = salaryReport.TotalDeductionAmount,
                NetSalary = salaryReport.NetSalary
            };
        }
    }
}