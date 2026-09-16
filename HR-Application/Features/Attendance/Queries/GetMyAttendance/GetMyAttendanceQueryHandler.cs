using HR_Application.Features.Attendance.DTOs;
using HR_Application.Interfaces.Persistence;
using HR_Application.Interfaces.Services;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace HR_Application.Features.Attendance.Queries.GetMyAttendance
{
    public record GetMyAttendanceQuery(AttendanceFilterDto Filter) : IRequest<List<AttendanceDto>>;

    public class GetMyAttendanceQueryHandler : IRequestHandler<GetMyAttendanceQuery, List<AttendanceDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly ICurrentUserService _currentUserService;

        public GetMyAttendanceQueryHandler(IApplicationDbContext context, ICurrentUserService currentUserService)
        {
            _context = context;
            _currentUserService = currentUserService;
        }

        public async Task<List<AttendanceDto>> Handle(GetMyAttendanceQuery request, CancellationToken cancellationToken)
        {
            var userEmail = _currentUserService.UserEmail;
            if (string.IsNullOrEmpty(userEmail)) return new();

            var employee = await _context.Employees
                .AsNoTracking()
                .FirstOrDefaultAsync(e => e.Email == userEmail, cancellationToken);

            if (employee == null) return new();

          
            var query = _context.Attendances
                .AsNoTracking()
                .Where(a => a.EmployeeId == employee.Id);

         
            if (request.Filter != null)
            {
                if (request.Filter.Month.HasValue)
                {
                    query = query.Where(a => a.Date.Month == request.Filter.Month.Value);
                }

                if (request.Filter.Year.HasValue)
                {
                    query = query.Where(a => a.Date.Year == request.Filter.Year.Value);
                }

                if (request.Filter.Date.HasValue)
                {
                    query = query.Where(a => a.Date.Date == request.Filter.Date.Value.Date);
                }

                if (request.Filter.FromDate.HasValue)
                {
                    query = query.Where(a => a.Date >= request.Filter.FromDate.Value);
                }

                if (request.Filter.ToDate.HasValue)
                {
                    query = query.Where(a => a.Date <= request.Filter.ToDate.Value);
                }
            }

     
            var attendanceRecords = await query
                .OrderByDescending(a => a.Date)
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
                    Status = a.Status,
                })
                .ToListAsync(cancellationToken);

            return attendanceRecords;
        }
    }
}