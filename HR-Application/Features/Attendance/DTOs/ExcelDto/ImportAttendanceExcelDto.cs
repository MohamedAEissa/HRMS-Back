using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.DTOs.ExcelDto
{
    public class ImportAttendanceExcelDto
    {
        [Required]
        public IFormFile File { get; set; } = null!;
    }
}
