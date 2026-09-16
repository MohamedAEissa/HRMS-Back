using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Auth.Commands.UpdateAccount
{
    public class UpdateUserAccountCommandValidator:AbstractValidator<UpdateUserAccountCommand>
    {
        public UpdateUserAccountCommandValidator()
        {
            RuleFor(x=>x.Id).NotEmpty().NotEmpty().WithMessage("User ID is required.");

            RuleFor(x => x.Dto.FullName)
                .NotEmpty().WithMessage("Full Name is required.")
                .MaximumLength(100).WithMessage("Full Name cannot exceed 100 characters.");

            RuleFor(x => x.Dto.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email address format.");

            RuleFor(x => x.Dto.RoleName)
                .NotEmpty().WithMessage("Role Name is required.");
        }
    }
}
