using HR_Application.Features.Employees.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Queries.GetMyProfile
{
    public record GetMyProfileQuery : IRequest<EmployeeResponseDto?>;
}
