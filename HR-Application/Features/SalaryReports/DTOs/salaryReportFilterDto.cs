using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.SalaryReports.DTOs
{
    public class salaryReportFilterDto
    {
        public Guid? EmployeeId { get; set; }
        public string? EmployeeName { get; set; }

        public Guid? DepartmentId { get; set; }
        public string? DepartmentName { get; set; }


        public int? Month { get; set; }
        public int? Year { get; set; }

      
    }
}
