using HR_Application.Features.Attendance.DTOs.ExcelDto;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Attendance.Commands.ImportAttendanceFromExcel
{
    public record ImportAttendanceExcelCommand(ImportAttendanceExcelDto Dto) : IRequest<ImportAttendanceResultDto>;
}
