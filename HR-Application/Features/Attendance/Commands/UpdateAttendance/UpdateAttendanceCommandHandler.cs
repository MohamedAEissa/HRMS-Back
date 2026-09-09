using HR_Application.Interfaces.Persistence;
using HR_Domain.Enums;
using MediatR;
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
            var attendance = await _context.Attendances.FindAsync(new object[] { request.Id }, cancellationToken);
            if (attendance == null) return false;

            var officialStartTime = new TimeSpan(8, 0, 0);   // 08:00 AM
            var officialEndTime = new TimeSpan(16, 0, 0);    // 04:00 PM

            decimal deductionHours = 0;
            decimal overtimeHours = 0;

           
            if (request.Dto.Status == AttendanceStatus.Absent ||
                request.Dto.Status == AttendanceStatus.OfficialHoliday ||
                request.Dto.Status == AttendanceStatus.WeeklyOff)
            {
                request.Dto.CheckInTime = null;
                request.Dto.CheckOutTime = null;
            }
            else
            {
                
                if (request.Dto.CheckInTime.HasValue && request.Dto.CheckInTime.Value > officialStartTime)
                {
                    var delay = request.Dto.CheckInTime.Value - officialStartTime;
                    deductionHours += (decimal)delay.TotalHours;
                }

              
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