using HR_Application.Features.Roles.DTOs;
using HR_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.Queries.GetAllRoles
{
    public class GetAllRolesQueryHandler : IRequestHandler<GetAllRolesQuery, List<RoleResponseDto>>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public GetAllRolesQueryHandler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<List<RoleResponseDto>> Handle(GetAllRolesQuery request, CancellationToken cancellationToken)
        {
            var roles = await _roleManager.Roles
                .AsNoTracking()
                .Select(r => new RoleResponseDto
                {
                    Id = r.Id,
                    RoleName = r.Name!,
                    Description = r.Description,
                    CreatedAt = r.CreatedAt
                })
                .ToListAsync(cancellationToken);

            return roles;
        }
    }
}