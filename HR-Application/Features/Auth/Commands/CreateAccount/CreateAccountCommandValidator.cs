using FluentValidation;
using HR_Application.Interfaces.Persistence;
using HR_Domain.Common;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Auth.Commands.CreateAccount
{
    public class CreateAccountCommandValidator : AbstractValidator<CreateAccountCommand>
    {
        private readonly IApplicationDbContext _context;

        public  CreateAccountCommandValidator(IApplicationDbContext context)
        {
            _context = context;

            RuleFor(x => x.Dto.Email)
                .NotEmpty().WithMessage("Email is required.")
                .EmailAddress().WithMessage("Invalid email format.")
                .MustAsync(async (email, cancellation) =>
                {
                    return await _context.Employees.AnyAsync(e => e.Email == email, cancellation);
                }).WithMessage("This email does not belong to any registered employee!")
                .MustAsync(async (email, cancellation) =>
                {
                    return !await _context.ApplicationUser.AnyAsync(u => u.Email == email, cancellation);
                }).WithMessage("An account for this employee already exists!");

            RuleFor(x => x.Dto.Password)
                .NotEmpty().WithMessage("Password is required.")
                .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

            RuleFor(x => x.Dto.ConfirmPassword)
                .Equal(x => x.Dto.Password).WithMessage("Passwords do not match.");

           
        }
    }
}
