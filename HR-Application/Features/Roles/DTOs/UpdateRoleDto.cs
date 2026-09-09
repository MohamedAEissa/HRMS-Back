using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Roles.DTOs
{
    public class UpdateRoleDto
    {
        public Guid Id { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public string? Description { get; set; } = string.Empty;
    }   
}
