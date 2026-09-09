using HR_Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.DTOs
{
    public class UpdateAttendanceDto
    {
        public string Date { get; set; } = string.Empty;
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
        public AttendanceStatus Status { get; set; }
        
    }
}
 