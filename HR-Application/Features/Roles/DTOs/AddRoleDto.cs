using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.DTOs
{
    public class AddRoleDto
    {
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
    }
}
