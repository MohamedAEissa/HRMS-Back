using ExcelDataReader;
using HR_Application.Features.Attendance.Commands.ImportAttendanceFromExcel;
using HR_Application.Features.Attendance.DTOs;
using HR_Application.Features.Attendance.DTOs.ExcelDto;
using HR_Application.Interfaces.Persistence;
using HR_Domain.Enums;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.Commands.ImportAttendanceExcel
{
    public class ImportAttendanceExcelCommandHandler : IRequestHandler<ImportAttendanceExcelCommand, ImportAttendanceResultDto>
    {
        private readonly IApplicationDbContext _context;

        public ImportAttendanceExcelCommandHandler(IApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ImportAttendanceResultDto> Handle(ImportAttendanceExcelCommand request, CancellationToken cancellationToken)
        {
            var result = new ImportAttendanceResultDto();
            var newAttendances = new List<HR_Domain.Entities.Attendance>();

            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

            if (request.Dto.File == null || request.Dto.File.Length == 0)
            {
                result.Errors.Add(new RowErrorDto { RowNumber = 0, ErrorMessage = "File is empty or not provided." });
                return result;
            }

            // 1. جلب الموظفين ومواعيد عملهم
            var employeesMap = await _context.Employees
                .AsNoTracking()
                .ToDictionaryAsync(
                    e => e.Code.Trim().ToLower(),
                    e => new
                    {
                        e.Id,
                        OfficialStart = e.CheckInTime != TimeSpan.Zero ? e.CheckInTime : new TimeSpan(8, 0, 0),
                        OfficialEnd = e.CheckOutTime != TimeSpan.Zero ? e.CheckOutTime : new TimeSpan(16, 0, 0)
                    },
                    cancellationToken);

            // 2. جلب كافة سجلات الحضور المسبقة لتفادي الـ Duplicate
            var existingAttendances = await _context.Attendances
                .AsNoTracking()
                .Select(a => new { a.EmployeeId, Date = a.Date.Date })
                .ToListAsync(cancellationToken);

            var existingSet = new HashSet<(Guid EmployeeId, DateTime Date)>(
                existingAttendances.Select(a => (a.EmployeeId, a.Date))
            );

            using var stream = request.Dto.File.OpenReadStream();
            using var reader = ExcelReaderFactory.CreateReader(stream);

            int rowNumber = 0;

            // التجاوز عن Header Row
            reader.Read();

            while (reader.Read())
            {
                rowNumber++;
                result.TotalRows++;

                var empCode = reader.GetValue(0)?.ToString()?.Trim();
                var rawDate = reader.GetValue(1)?.ToString()?.Trim();
                var rawCheckInObj = reader.GetValue(2);
                var rawCheckOutObj = reader.GetValue(3);

                if (string.IsNullOrWhiteSpace(empCode) || string.IsNullOrWhiteSpace(rawDate))
                {
                    result.FailedCount++;
                    result.Errors.Add(new RowErrorDto
                    {
                        RowNumber = rowNumber,
                        EmployeeCode = empCode ?? "N/A",
                        ErrorMessage = "Employee code or Date is missing."
                    });
                    continue;
                }

                if (!employeesMap.TryGetValue(empCode.ToLower(), out var employeeInfo))
                {
                    result.FailedCount++;
                    result.Errors.Add(new RowErrorDto
                    {
                        RowNumber = rowNumber,
                        EmployeeCode = empCode,
                        ErrorMessage = $"Employee with Code '{empCode}' was not found."
                    });
                    continue;
                }

                if (!DateTime.TryParse(rawDate, out DateTime attendanceDate))
                {
                    result.FailedCount++;
                    result.Errors.Add(new RowErrorDto
                    {
                        RowNumber = rowNumber,
                        EmployeeCode = empCode,
                        ErrorMessage = $"Invalid Date format '{rawDate}'."
                    });
                    continue;
                }

                attendanceDate = attendanceDate.Date;

                bool isAlreadyRecorded = existingSet.Contains((employeeInfo.Id, attendanceDate))
                    || newAttendances.Any(a => a.EmployeeId == employeeInfo.Id && a.Date == attendanceDate);

                if (isAlreadyRecorded)
                {
                    result.FailedCount++;
                    result.Errors.Add(new RowErrorDto
                    {
                        RowNumber = rowNumber,
                        EmployeeCode = empCode,
                        ErrorMessage = $"Attendance record for date '{attendanceDate:yyyy-MM-dd}' already exists."
                    });
                    continue;
                }

                // --- Parsing المرن للوقت من Excel ---
                TimeSpan? checkInTime = ParseExcelTime(rawCheckInObj);
                TimeSpan? checkOutTime = ParseExcelTime(rawCheckOutObj);

                decimal deductionHours = 0;
                decimal overtimeHours = 0;

                var officialStartTime = employeeInfo.OfficialStart;
                var officialEndTime = employeeInfo.OfficialEnd;

                // 3. حساب الـ Status والخصومات
                AttendanceStatus computedStatus;

                if (!checkInTime.HasValue)
                {
                    computedStatus = AttendanceStatus.Absent;
                }
                else if (checkInTime.Value > officialStartTime)
                {
                    computedStatus = AttendanceStatus.late;

                    var delay = checkInTime.Value - officialStartTime;
                    deductionHours += (decimal)delay.TotalHours;
                }
                else
                {
                    computedStatus = AttendanceStatus.Present;
                }

                if (checkOutTime.HasValue)
                {
                    if (checkOutTime.Value > officialEndTime)
                    {
                        var over = checkOutTime.Value - officialEndTime;
                        overtimeHours = (decimal)over.TotalHours;
                    }
                    else if (checkOutTime.Value < officialEndTime)
                    {
                        var earlyLeave = officialEndTime - checkOutTime.Value;
                        deductionHours += (decimal)earlyLeave.TotalHours;
                    }
                }

                var attendance = new HR_Domain.Entities.Attendance
                {
                    EmployeeId = employeeInfo.Id,
                    Date = attendanceDate,
                    CheckInTime = checkInTime,
                    CheckOutTime = checkOutTime,
                    Status = computedStatus,
                    DeductionHours = Math.Round(deductionHours, 2),
                    OvertimeHours = Math.Round(overtimeHours, 2)
                };

                newAttendances.Add(attendance);
                result.SuccessCount++;
            }

            if (newAttendances.Any())
            {
                await _context.Attendances.AddRangeAsync(newAttendances, cancellationToken);
                await _context.SaveChangesAsync(cancellationToken);
            }

            return result;
        }

        // Helper Method لتحويل أشكال قيم Excel المخزنة إلى TimeSpan دقيق
        private static TimeSpan? ParseExcelTime(object obj)
        {
            if (obj == null) return null;

            if (obj is DateTime dt)
            {
                if (dt.TimeOfDay == TimeSpan.Zero) return null; // 00:00:00 تعني غياب
                return dt.TimeOfDay;
            }

            var str = obj.ToString()?.Trim();
            if (string.IsNullOrWhiteSpace(str) || str == "0:00:00" || str == "00:00:00")
            {
                return null;
            }

            if (TimeSpan.TryParse(str, out var ts))
            {
                if (ts == TimeSpan.Zero) return null;
                return ts;
            }

            if (DateTime.TryParse(str, out var parsedDt))
            {
                if (parsedDt.TimeOfDay == TimeSpan.Zero) return null;
                return parsedDt.TimeOfDay;
            }

            return null;
        }
    }
}