using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.DTOs
{
    public class GetFilteredAttendanceDto
    {
        public string? EmployeeName { get; set; }
        public int? Month { get; set; }
        public int? Year { get; set; }
        public DateTime? Date { get; set; }
    }
}
