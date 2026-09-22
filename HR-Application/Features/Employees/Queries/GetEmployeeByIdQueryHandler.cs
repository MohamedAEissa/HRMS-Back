using AutoMapper;
using HR_Application.Features.Employees.DTOs;
using HR_Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Queries
{
    public record GetEmployeeByIdQuery(Guid Id) : IRequest<EmployeeResponseDto>;

    public class GetEmployeeByIdQueryHandler : IRequestHandler<GetEmployeeByIdQuery, EmployeeResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public GetEmployeeByIdQueryHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EmployeeResponseDto> Handle(GetEmployeeByIdQuery request, CancellationToken cancellationToken)
        {
            var employee = await _context.Employees
                 .AsNoTracking()
                 .Include(e => e.Department)
                 .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID '{request.Id}' was not found.");

            return _mapper.Map<EmployeeResponseDto>(employee);
        }
    }
}