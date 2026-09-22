using HR_Application.Interfaces.Persistence;
using HR_Domain.Entities;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Infrastructure.Context
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>, IApplicationDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

          
            builder.Entity<Employee>()
                .HasOne(e => e.User)
                .WithOne(u => u.Employee)
                .HasForeignKey<Employee>(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<Employee>()
                .HasOne(e => e.Department)
                .WithMany(d => d.Employees)
                .HasForeignKey(e => e.DepartmentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<Attendance>()
                .HasOne(a => a.Employee)
                .WithMany()
                .HasForeignKey(a => a.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<SalaryReport>()
                .HasOne(s => s.Employee)
                .WithMany()
                .HasForeignKey(s => s.EmployeeId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<Employee> Employees => Set<Employee>();
        public DbSet<Department> Departments => Set<Department>();
        public DbSet<GeneralSettings> GeneralSettings => Set<GeneralSettings>();
        public DbSet<OfficialHoliday> OfficialHolidays => Set<OfficialHoliday>();
        public DbSet<Attendance> Attendances => Set<Attendance>();
        public DbSet<SalaryReport> SalaryReports => Set<SalaryReport>();

        public DbSet<ApplicationUser> ApplicationUser => Set<ApplicationUser>();
        public DbSet<ApplicationRole> ApplicationRoles => Set<ApplicationRole>();
    }
}