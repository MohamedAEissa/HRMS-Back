using FluentValidation;
using HR_Application.Interfaces.Persistence;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandValidator : AbstractValidator<CreateEmployeeCommand>
    {
        private readonly IApplicationDbContext _context;

        public CreateEmployeeCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            // 1. FullName Validation
            RuleFor(x => x.dto.FullName)
                .NotEmpty().WithMessage("Full name is required.")
                .Length(2, 100).WithMessage("Full name must be between 2 and 100 characters.")
                .Matches(@"^[a-zA-Z\sأ-يء-ئ]+$").WithMessage("Full name can only contain letters and spaces.")
                .MustAsync(BeUniqueFullName).WithMessage("This FullName ID is already registered.");

            // 2. NationalId Validation (Structural & Unique Validation)
            RuleFor(x => x.dto.NationalId)
                .NotEmpty().WithMessage("National ID is required.")
                .Length(14).WithMessage("National ID must be exactly 14 digits.")
                .Must(BeAValidEgyptianNationalId).WithMessage("Invalid Egyptian National ID structure, date, or governorate code.")
                .MustAsync(BeUniqueNationalId).WithMessage("This National ID is already registered.");

            // 3. Email Validation
            RuleFor(x => x.dto.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MustAsync(BeUniqueEmail).WithMessage("This email is already registered.");

            // 4. Phone Validation 
            RuleFor(x => x.dto.Phone)
                .NotEmpty().WithMessage("Phone number is required.")
                .Matches(@"^01[0125][0-9]{8}$").WithMessage("Invalid Egyptian phone number format.");

            // 5. Address Validation
            RuleFor(x => x.dto.Address)
                .NotEmpty().WithMessage("Address is required.")
                .MinimumLength(5).WithMessage("Address must be at least 5 characters.");

            // 6. Nationality Validation
            RuleFor(x => x.dto.Nationality)
                .NotEmpty().WithMessage("Nationality is required.");

            // 7. Gender Validation 
            RuleFor(x => x.dto.Gender)
                .NotEmpty().WithMessage("Gender is required.")
                .Must(gender => gender == "Male" || gender == "Female").WithMessage("Gender must be either 'Male' or 'Female'.");

            // 8. BirthDate Validation 
            RuleFor(x => x.dto.BirthDate)
                .NotEmpty().WithMessage("Birth date is required.")
                .LessThan(DateTime.Today.AddYears(-18)).WithMessage("Employee must be at least 18 years old.")
                .GreaterThan(DateTime.Today.AddYears(-70)).WithMessage("Invalid birth date.");

            // 9. Salary Validation
            RuleFor(x => x.dto.Salary)
                .NotEmpty().WithMessage("Salary is required.")
                .GreaterThan(0).WithMessage("Salary must be greater than zero.")
                .LessThan(1000000).WithMessage("Salary is unrealistically high.");

            // 10. ContractDate Validation
            RuleFor(x => x.dto.ContractDate)
                .NotEmpty().WithMessage("Contract date is required.")
                .LessThanOrEqualTo(DateTime.Today).WithMessage("Contract date cannot be in the future.");

            // 11. Check-In & Check-Out Time
            RuleFor(x => x.dto.CheckInTime)
                .NotEmpty().WithMessage("Check-in time is required.");

            RuleFor(x => x.dto.CheckOutTime)
                .NotEmpty().WithMessage("Check-out time is required.")
                .Must((command, checkOut) => checkOut > command.dto.CheckInTime)
                .WithMessage("Check-out time must be after check-in time.");

            // 12. DepartmentId Validation
            RuleFor(x => x.dto.DepartmentId)
                .NotEmpty().WithMessage("Department ID is required.")
                .MustAsync(DepartmentExists).WithMessage("Selected department does not exist.");
        }


        private bool BeAValidEgyptianNationalId(string nationalId)
        {
            if (string.IsNullOrEmpty(nationalId) || nationalId.Length != 14 || !long.TryParse(nationalId, out _))
                return false;

            // 1. التحقق من الرقم الأول (قرن الميلاد: 2 لمواليد 1900، 3 لمواليد 2000)
            string centuryDigit = nationalId.Substring(0, 1);
            if (centuryDigit != "2" && centuryDigit != "3")
                return false;

            int yearPrefix = centuryDigit == "2" ? 19 : 20;
            int year = yearPrefix * 100 + int.Parse(nationalId.Substring(1, 2));
            int month = int.Parse(nationalId.Substring(3, 2));
            int day = int.Parse(nationalId.Substring(5, 2));

            // التحقق من صحة تاريخ الميلاد المستخرج من الرقم
            if (!DateTime.TryParse($"{year}-{month:D2}-{day:D2}", out _))
                return false;

            // 2. التحقق من كود المحافظة (الرقم الثامن والتاسع)
            string govCode = nationalId.Substring(7, 2);
            string[] validGovernorates = {
                "01", "02", "03", "04", "11", "12", "13", "14", "15", "16",
                "17", "18", "19", "21", "22", "23", "24", "25", "26", "27",
                "28", "29", "31", "32", "33", "34", "35", "88"
            };

            if (!validGovernorates.Contains(govCode))
                return false;

            return true;
        }
         
        // --- دوال التحقق المساعدة (Async Custom Rules) ---

        private async Task<bool> BeUniqueEmail(string email, CancellationToken cancellationToken)
        {
            return !await _context.Employees.AnyAsync(e => e.Email == email, cancellationToken);
        }

        private async Task<bool> BeUniqueFullName(string fullName, CancellationToken cancellationToken)
        {
            return !await _context.Employees.AnyAsync(e => e.FullName == fullName, cancellationToken);
        }

        private async Task<bool> BeUniqueNationalId(string nationalId, CancellationToken cancellationToken)
        {
            return !await _context.Employees.AnyAsync(e => e.NationalId == nationalId, cancellationToken);
        }

        private async Task<bool> DepartmentExists(Guid departmentId, CancellationToken cancellationToken)
        {
            return await _context.Departments.AnyAsync(d => d.Id == departmentId, cancellationToken);
        }
    }
}