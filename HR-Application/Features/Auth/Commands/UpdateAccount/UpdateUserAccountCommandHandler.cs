using AutoMapper;
using HR_Application.Features.Auth.DTOs;
using HR_Application.Interfaces.Persistence;
using HR_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Auth.Commands.UpdateAccount
{
    public class UpdateUserAccountCommandHandler : IRequestHandler<UpdateUserAccountCommand, UserAccountDto>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager;
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UpdateUserAccountCommandHandler(
            UserManager<ApplicationUser> userManager,
            RoleManager<ApplicationRole> roleManager,
            IApplicationDbContext context,
            IMapper mapper)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _context = context;
            _mapper = mapper;
        }

        public async Task<UserAccountDto> Handle(UpdateUserAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id.ToString())
                ?? throw new KeyNotFoundException($"User with ID '{request.Id}' was not found.");

            if (!string.Equals(user.Email, request.Dto.Email, StringComparison.OrdinalIgnoreCase))
            {
                var existingUser = await _userManager.FindByEmailAsync(request.Dto.Email);
                if (existingUser != null && existingUser.Id != user.Id)
                {
                    throw new InvalidOperationException($"Email '{request.Dto.Email}' is already taken.");
                }

                user.Email = request.Dto.Email;
                user.NormalizedEmail = request.Dto.Email.ToUpperInvariant();
            }

            user.FullName = request.Dto.FullName;
            user.IsActive = request.Dto.IsActive;

            var updateResult = await _userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                var errors = string.Join(", ", updateResult.Errors.Select(e => e.Description));
                throw new Exception($"Failed to update user account: {errors}");
            }

            if (!string.IsNullOrWhiteSpace(request.Dto.RoleName))
            {
                var roleExists = await _roleManager.RoleExistsAsync(request.Dto.RoleName);
                if (!roleExists)
                {
                    throw new KeyNotFoundException($"Role '{request.Dto.RoleName}' does not exist.");
                }

                var currentRoles = await _userManager.GetRolesAsync(user);
                await _userManager.RemoveFromRolesAsync(user, currentRoles);
                await _userManager.AddToRoleAsync(user, request.Dto.RoleName);
            }

            await _context.SaveChangesAsync(cancellationToken);

            var updatedRoles = await _userManager.GetRolesAsync(user);
            var responseDto = _mapper.Map<UserAccountDto>(user);

            responseDto.RoleName = updatedRoles.FirstOrDefault() ?? string.Empty;

            return responseDto;
        }
    }
}
