using HR_Application.Features.Roles.DTOs;
using HR_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.Commands.AddRole
{
    public class AddRoleCommandHandler : IRequestHandler<AddRoleCommand, RoleResponseDto>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public AddRoleCommandHandler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<RoleResponseDto> Handle(AddRoleCommand request, CancellationToken cancellationToken)
        {
          
            var roleExists = await _roleManager.RoleExistsAsync(request.Dto.RoleName);
            if (roleExists)
                throw new InvalidOperationException($"Role '{request.Dto.RoleName}' already exists.");
            

            var role = new ApplicationRole(request.Dto.RoleName, request.Dto.Description)
            {
                CreatedAt = DateTime.UtcNow
            };

            
            var result = await _roleManager.CreateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to create role: {errors}");
            }

            return new RoleResponseDto
            {
                Id = role.Id,
                RoleName = role.Name!,
                CreatedAt = role.CreatedAt,
                Description = role.Description
            };
        }
    }
}