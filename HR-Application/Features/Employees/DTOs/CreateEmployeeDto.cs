using System;

namespace HR_Application.Features.Employees.DTOs
{
    public class CreateEmployeeDto
    {
        public string FullName { get; set; } = string.Empty;
        public string NationalId { get; set; } = string.Empty;
        public string Address { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Phone { get; set; } = string.Empty;
        public string Nationality { get; set; } = string.Empty;
        public string Gender { get; set; } = string.Empty;
        public DateTime BirthDate { get; set; }
        public decimal Salary { get; set; }
        public DateTime ContractDate { get; set; }
        public TimeSpan CheckInTime { get; set; } = new TimeSpan(8, 0, 0);   // Default 08:00 AM
        public TimeSpan CheckOutTime { get; set; } = new TimeSpan(16, 0, 0);  // Default 04:00 PM
        public Guid DepartmentId { get; set; }
    }
}