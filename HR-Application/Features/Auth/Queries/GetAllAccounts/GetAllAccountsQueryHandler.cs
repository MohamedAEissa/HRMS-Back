
using HR_Application.Features.Auth.DTOs;
using HR_Domain.Entities; // التأكد من عمل Import لـ ApplicationUser و ApplicationRole
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Accounts.Queries.GetAllAccounts
{
    public record GetAllAccountsQuery : IRequest<List<UserAccountDto>>;

    public class GetAllAccountsQueryHandler : IRequestHandler<GetAllAccountsQuery, List<UserAccountDto>>
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly RoleManager<ApplicationRole> _roleManager; 

        public GetAllAccountsQueryHandler(UserManager<ApplicationUser> userManager, RoleManager<ApplicationRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<List<UserAccountDto>> Handle(GetAllAccountsQuery request, CancellationToken cancellationToken)
        {
            var users = await _userManager.Users.ToListAsync(cancellationToken);
            var accountList = new List<UserAccountDto>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                accountList.Add(new UserAccountDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName ?? string.Empty,
                    Email = user.Email ?? string.Empty,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt,
                    RoleName = roles.FirstOrDefault() ?? "Employee"
                });
            }

            return accountList;
        }
    }
}