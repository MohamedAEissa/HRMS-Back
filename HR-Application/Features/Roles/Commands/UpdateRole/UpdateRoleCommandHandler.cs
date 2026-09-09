using HR_Application.Features.Roles.DTOs;
using HR_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.Commands.UpdateRole
{
    public class UpdateRoleCommandHandler : IRequestHandler<UpdateRoleCommand, RoleResponseDto>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;

        public UpdateRoleCommandHandler(RoleManager<ApplicationRole> roleManager)
        {
            _roleManager = roleManager;
        }

        public async Task<RoleResponseDto> Handle(UpdateRoleCommand request, CancellationToken cancellationToken)
        {
          
            var role = await _roleManager.FindByIdAsync(request.Dto.Id.ToString());
            if (role == null)
                throw new KeyNotFoundException($"Role with ID '{request.Dto.Id}' was not found.");
            

           
            var existingRole = await _roleManager.FindByNameAsync(request.Dto.RoleName);

            if (existingRole != null && existingRole.Id != role.Id)
                throw new InvalidOperationException($"Role name '{request.Dto.RoleName}' is already taken.");
          

            
            role.Name = request.Dto.RoleName;
            role.Description = request.Dto.Description;

            
            var result = await _roleManager.UpdateAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update role: {errors}");
            }

            return new RoleResponseDto
            {
                Id = role.Id,
                RoleName = role.Name,
                Description = role.Description,
                CreatedAt = role.CreatedAt
            };
        }
    }
}