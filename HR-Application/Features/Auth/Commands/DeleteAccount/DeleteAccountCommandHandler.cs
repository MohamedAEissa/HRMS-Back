using HR_Domain.Entities;
using MediatR;
using Microsoft.AspNetCore.Identity;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Accounts.Commands.DeleteAccount
{
    public record DeleteAccountCommand(string Id) : IRequest<bool>;

    public class DeleteAccountCommandHandler : IRequestHandler<DeleteAccountCommand, bool>
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public DeleteAccountCommandHandler(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<bool> Handle(DeleteAccountCommand request, CancellationToken cancellationToken)
        {
            var user = await _userManager.FindByIdAsync(request.Id);
            if (user == null) throw new Exception("User not found.");

            var result = await _userManager.DeleteAsync(user);
            return result.Succeeded;
        }
    }
}