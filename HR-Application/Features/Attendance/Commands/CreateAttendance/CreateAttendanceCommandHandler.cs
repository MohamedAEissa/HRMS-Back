using HR_Application.Interfaces.Persistence;
using HR_Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.Commands.CreateAttendance
{
    public class CreateAttendanceCommandHandler : IRequestHandler<CreateAttendanceCommand, Guid>
    {
        private readonly IApplicationDbContext _context;

        public CreateAttendanceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> Handle(CreateAttendanceCommand request, CancellationToken cancellationToken)
        {
            // 1. التثبت من وجود الموظف وقراءة مواعيد العمل الخاصة به
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == request.Dto.EmployeeId, cancellationToken);

            if (employee == null)
            {
                throw new KeyNotFoundException($"Employee with ID '{request.Dto.EmployeeId}' was not found.");
            }

            // 2. تحديد مواعيد العمل الرسمية للموظف (ديناميكياً أو افتراضياً 8 و 16)
            var officialStartTime = employee.CheckInTime != TimeSpan.Zero
                ? employee.CheckInTime
                : new TimeSpan(8, 0, 0);   // 08:00 AM

            var officialEndTime = employee.CheckOutTime != TimeSpan.Zero
                ? employee.CheckOutTime
                : new TimeSpan(16, 0, 0);   // 04:00 PM

            decimal deductionHours = 0;
            decimal overtimeHours = 0;

            // 3. معالجة حالات الغياب والتأخير والإضافي
            if (request.Dto.Status == AttendanceStatus.Absent ||
                request.Dto.Status == AttendanceStatus.OfficialHoliday ||
                request.Dto.Status == AttendanceStatus.WeeklyOff)
            {
                request.Dto.CheckInTime = null;
                request.Dto.CheckOutTime = null;
            }
            else
            {
                // حساب التأخير في الحضور
                if (request.Dto.CheckInTime.HasValue && request.Dto.CheckInTime.Value > officialStartTime)
                {
                    var delay = request.Dto.CheckInTime.Value - officialStartTime;
                    deductionHours += (decimal)delay.TotalHours;
                }

                // حساب الإضافي أو الانصراف المبكر
                if (request.Dto.CheckOutTime.HasValue)
                {
                    if (request.Dto.CheckOutTime.Value > officialEndTime)
                    {
                        var over = request.Dto.CheckOutTime.Value - officialEndTime;
                        overtimeHours = (decimal)over.TotalHours;
                    }
                    else if (request.Dto.CheckOutTime.Value < officialEndTime)
                    {
                        var earlyLeave = officialEndTime - request.Dto.CheckOutTime.Value;
                        deductionHours += (decimal)earlyLeave.TotalHours;
                    }
                }
            }

            deductionHours = Math.Round(deductionHours, 2);
            overtimeHours = Math.Round(overtimeHours, 2);

            var attendance = new HR_Domain.Entities.Attendance
            {
                EmployeeId = request.Dto.EmployeeId,
                Date = Convert.ToDateTime(request.Dto.Date).Date,
                CheckInTime = request.Dto.CheckInTime,
                CheckOutTime = request.Dto.CheckOutTime,
                Status = request.Dto.Status,
                DeductionHours = deductionHours,
                OvertimeHours = overtimeHours
            };

            _context.Attendances.Add(attendance);
            await _context.SaveChangesAsync(cancellationToken);

            return attendance.Id;
        }
    }
}