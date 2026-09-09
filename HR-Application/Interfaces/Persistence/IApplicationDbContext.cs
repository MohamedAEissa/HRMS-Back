using HR_Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Interfaces.Persistence
{
    public interface IApplicationDbContext
    {
        DbSet<Employee> Employees { get; }
        DbSet<Department> Departments { get; }
        DbSet<GeneralSettings> GeneralSettings { get; }
        DbSet<OfficialHoliday> OfficialHolidays { get; }
        DbSet<Attendance> Attendances { get; }
        DbSet<SalaryReport> SalaryReports { get; }
        DbSet<ApplicationUser> ApplicationUser { get; }
        DbSet<ApplicationRole> ApplicationRoles { get; }

        Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
