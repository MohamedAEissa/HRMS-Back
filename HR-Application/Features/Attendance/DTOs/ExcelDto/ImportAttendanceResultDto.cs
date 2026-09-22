using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.DTOs.ExcelDto
{
    public class ImportAttendanceResultDto
    {
        public int TotalRows { get; set; }
        public int SuccessCount { get; set; }
        public int FailedCount { get; set; }
        public List<RowErrorDto> Errors { get; set; } = new List<RowErrorDto>();
    }

    public class RowErrorDto
    {
        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; } = string.Empty;
        public string ErrorMessage { get; set; } = string.Empty;
    }
}
