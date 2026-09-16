using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Interfaces.Services
{
    public interface ICurrentUserService
    {
        string? UserId { get; }
        string? UserName { get; }
        string? UserEmail { get; }
        bool IsAuthenticated { get; }
        public bool IsInRole(string role);
    }
}
