using HR_Application.Features.SalaryReports.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.SalaryReports.Queries.GetSalaryReports
{
    public record GetSalaryReportsQuery(salaryReportFilterDto Dto) : IRequest<List<SalaryReporResponsetDto>>;
}
