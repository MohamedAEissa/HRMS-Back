using AutoMapper;
using HR_Application.Features.Employees.DTOs;
using HR_Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Queries
{
    public record GetAllEmployeesQuery : IRequest<List<EmployeeResponseDto>>;

    public class GetAllEmployeesQueryHandler : IRequestHandler<GetAllEmployeesQuery, List<EmployeeResponseDto>>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetAllEmployeesQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<List<EmployeeResponseDto>> Handle(GetAllEmployeesQuery request, CancellationToken cancellationToken)
        {
            var employees = await _context.Employees
                .AsNoTracking()
                .Include(e => e.Department)
                .ToListAsync(cancellationToken);

            return _mapper.Map<List<EmployeeResponseDto>>(employees);
        }
    }
}