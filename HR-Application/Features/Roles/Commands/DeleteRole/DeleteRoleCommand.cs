using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.Commands.DeleteRole
{
    public record DeleteRoleCommand(Guid Id) : IRequest<bool>;
}
