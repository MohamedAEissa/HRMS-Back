using System;

namespace HR_Application.Features.Employees.DTOs
{
    public class UpdateEmployeeDto
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
        public TimeSpan CheckInTime { get; set; }
        public TimeSpan CheckOutTime { get; set; }
        public Guid DepartmentId { get; set; }
    }
}