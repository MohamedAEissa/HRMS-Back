using HR_Application.Features.Attendance.DTOs;
using HR_Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.Queries.GetAttendances
{
    public class GetAttendancesQueryHandler : IRequestHandler<GetAttendancesQuery, List<AttendanceDto>>
    {
        private readonly IApplicationDbContext _context;

        public GetAttendancesQueryHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<AttendanceDto>> Handle(GetAttendancesQuery request, CancellationToken cancellationToken)
        {
            var query = _context.Attendances
                .Include(a => a.Employee)
                .ThenInclude(e => e.Department)
                .AsNoTracking()
                .AsQueryable();

            var filter = request.Filter;

            if (filter != null)
            {
                
                if (filter.EmployeeId.HasValue)
                {
                    query = query.Where(a => a.EmployeeId == filter.EmployeeId.Value);
                }

                
                if (!string.IsNullOrWhiteSpace(filter.EmployeeName))
                {
                    query = query.Where(a => a.Employee.FullName != null &&
                                             a.Employee.FullName.ToLower().Contains(filter.EmployeeName.ToLower()));
                }

               
                if (filter.DepartmentId.HasValue)
                {
                    query = query.Where(a => a.Employee.DepartmentId == filter.DepartmentId.Value);
                }

              
                if (!string.IsNullOrWhiteSpace(filter.DepartmentName))
                {
                    query = query.Where(a => a.Employee.Department != null &&
                                             a.Employee.Department.Name.ToLower().Contains(filter.DepartmentName.ToLower()));
                }

                
                if (filter.Month.HasValue && filter.Month > 0)
                {
                    query = query.Where(a => a.Date.Month == filter.Month.Value);
                }

               
                if (filter.Year.HasValue && filter.Year > 0)
                {
                    query = query.Where(a => a.Date.Year == filter.Year.Value);
                }

               
                if (filter.Date.HasValue)
                {
                    query = query.Where(a => a.Date.Date == filter.Date.Value.Date);
                }

               
                if (filter.FromDate.HasValue)
                {
                    query = query.Where(a => a.Date >= filter.FromDate.Value.Date);
                }

                if (filter.ToDate.HasValue)
                {
                    query = query.Where(a => a.Date <= filter.ToDate.Value.Date);
                }
            }

            return await query
                .Select(a => new AttendanceDto
                {
                    Id = a.Id,
                    EmployeeId = a.EmployeeId,
                    EmployeeName = a.Employee.FullName,
                    DepartmentName = a.Employee.Department != null ? a.Employee.Department.Name : "No Dept",
                    Date = a.Date,
                    CheckInTime = a.CheckInTime,
                    CheckOutTime = a.CheckOutTime,
                    OvertimeHours = a.OvertimeHours,
                    DeductionHours = a.DeductionHours,
                    Status = a.Status
                })
                .OrderByDescending(a => a.Date)
                .ToListAsync(cancellationToken);
        }
    }
}