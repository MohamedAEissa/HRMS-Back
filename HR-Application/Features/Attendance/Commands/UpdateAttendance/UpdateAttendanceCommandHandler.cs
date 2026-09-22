using HR_Application.Interfaces.Persistence;
using HR_Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.Commands.UpdateAttendance
{
    public class UpdateAttendanceCommandHandler : IRequestHandler<UpdateAttendanceCommand, bool>
    {
        private readonly IApplicationDbContext _context;

        public UpdateAttendanceCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<bool> Handle(UpdateAttendanceCommand request, CancellationToken cancellationToken)
        {
            var attendance = await _context.Attendances
                .FirstOrDefaultAsync(a => a.Id == request.Id, cancellationToken);

            if (attendance == null) return false;

            // 1. جلب بيانات الموظف لمعرفة مواعيد عمله الرسمية
            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Id == attendance.EmployeeId, cancellationToken);

            if (employee == null) return false;

            // 2. تحديد مواعيد العمل الرسمية للموظف (ديناميكياً أو 8 و 16 كـ Default)
            var officialStartTime = employee.CheckInTime != TimeSpan.Zero
                ? employee.CheckInTime
                : new TimeSpan(8, 0, 0);   // 08:00 AM

            var officialEndTime = employee.CheckOutTime != TimeSpan.Zero
                ? employee.CheckOutTime
                : new TimeSpan(16, 0, 0);   // 04:00 PM

            decimal deductionHours = 0;
            decimal overtimeHours = 0;

            // 3. حساب التأخير والحسم والإضافي بناءً على الحالة والمواعيد الديناميكية
            if (request.Dto.Status == AttendanceStatus.Absent ||
                request.Dto.Status == AttendanceStatus.OfficialHoliday ||
                request.Dto.Status == AttendanceStatus.WeeklyOff)
            {
                request.Dto.CheckInTime = null;
                request.Dto.CheckOutTime = null;
            }
            else
            {
                // حساب تأخير الحضور
                if (request.Dto.CheckInTime.HasValue && request.Dto.CheckInTime.Value > officialStartTime)
                {
                    var delay = request.Dto.CheckInTime.Value - officialStartTime;
                    deductionHours += (decimal)delay.TotalHours;
                }

                // حساب وقت الانصراف (إضافي أو خروج مبكر)
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

            // 4. تحديث البيانات
            attendance.Date = Convert.ToDateTime(request.Dto.Date).Date;
            attendance.CheckInTime = request.Dto.CheckInTime;
            attendance.CheckOutTime = request.Dto.CheckOutTime;
            attendance.OvertimeHours = Math.Round(overtimeHours, 2);
            attendance.DeductionHours = Math.Round(deductionHours, 2);
            attendance.Status = request.Dto.Status;

            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}