using HR_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.Commands.DeleteRole
{
    public class DeleteRoleCommandHandler : IRequestHandler<DeleteRoleCommand, bool>
    {
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteRoleCommandHandler(
            RoleManager<ApplicationRole> roleManager,
            UserManager<ApplicationUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        public async Task<bool> Handle(DeleteRoleCommand request, CancellationToken cancellationToken)
        {
           
            var role = await _roleManager.FindByIdAsync(request.Id.ToString());
            if (role == null)
                throw new KeyNotFoundException($"Role with ID '{request.Id}' was not found.");
                

            // Make Sure that No user Related for this role 
            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);
            if (usersInRole.Any())
            {
                throw new InvalidOperationException($"Cannot delete role '{role.Name}' because it is assigned to {usersInRole.Count} user(s).");
            }

           
            var result = await _roleManager.DeleteAsync(role);

            if (!result.Succeeded)
            {
                var errors = string.Join(", ", result.Errors.Select(e => e.Description));
                throw new Exception($"Failed to delete role: {errors}");
            }

            return true;
        }
    }
}