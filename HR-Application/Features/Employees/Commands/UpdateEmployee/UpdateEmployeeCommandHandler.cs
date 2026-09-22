using AutoMapper;
using HR_Application.Features.Employees.DTOs;
using HR_Application.Interfaces.Persistence;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Commands.UpdateEmployee
{
    public class UpdateEmployeeCommandHandler : IRequestHandler<UpdateEmployeeCommand, EmployeeResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public UpdateEmployeeCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EmployeeResponseDto> Handle(UpdateEmployeeCommand request, CancellationToken cancellationToken)
        {
            var employee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == request.Id, cancellationToken);

            if (employee == null)
                throw new KeyNotFoundException($"Employee with ID '{request.Id}' was not found.");

            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.Dto.DepartmentId, cancellationToken);

            if (department == null)
                throw new KeyNotFoundException($"Department with ID '{request.Dto.DepartmentId}' was not found.");

           
            bool isEmailTaken = await _context.Employees
                .AnyAsync(e => e.Email == request.Dto.Email && e.Id != request.Id, cancellationToken);
            if (isEmailTaken)
                throw new InvalidOperationException($"Email '{request.Dto.Email}' is already in use by another employee.");

            bool isNationalIdTaken = await _context.Employees
                .AnyAsync(e => e.NationalId == request.Dto.NationalId && e.Id != request.Id, cancellationToken);
            if (isNationalIdTaken)
                throw new InvalidOperationException($"National ID '{request.Dto.NationalId}' is already in use by another employee.");

        
            _mapper.Map(request.Dto, employee);

            await _context.SaveChangesAsync(cancellationToken);

            return _mapper.Map<EmployeeResponseDto>(employee);
        }
    }
}