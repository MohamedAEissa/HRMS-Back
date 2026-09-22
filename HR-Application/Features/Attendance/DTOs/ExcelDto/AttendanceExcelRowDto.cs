using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.DTOs.ExcelDto
{
    public class AttendanceExcelRowDto
    {
        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string Date { get; set; } = string.Empty;
        public TimeSpan? CheckInTime { get; set; }
        public TimeSpan? CheckOutTime { get; set; }
    }
}
