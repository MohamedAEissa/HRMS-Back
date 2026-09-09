using HR_Application.Features.Roles.DTOs;
using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.Queries.GetAllRoles
{
    public record GetAllRolesQuery : IRequest<List<RoleResponseDto>>;
}
