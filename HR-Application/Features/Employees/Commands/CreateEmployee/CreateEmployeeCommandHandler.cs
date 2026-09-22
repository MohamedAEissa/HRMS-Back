using AutoMapper;
using HR_Application.Features.Employees.DTOs;
using HR_Application.Interfaces.Persistence;
using HR_Domain.Entities;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Commands.CreateEmployee
{
    public class CreateEmployeeCommandHandler : IRequestHandler<CreateEmployeeCommand, EmployeeResponseDto>
    {
        private readonly IApplicationDbContext _context;
        private readonly IMapper _mapper;

        public CreateEmployeeCommandHandler(IApplicationDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        public async Task<EmployeeResponseDto> Handle(CreateEmployeeCommand request, CancellationToken cancellationToken)
        {
            // 1. التحقق من وجود القسم
            var department = await _context.Departments
                .FirstOrDefaultAsync(d => d.Id == request.dto.DepartmentId, cancellationToken);

            if (department == null)
                throw new KeyNotFoundException($"Department with ID '{request.dto.DepartmentId}' was not found.");

            // 2. التحقق من عدم تكرار البريد الإلكتروني
            bool isEmailExists = await _context.Employees.AnyAsync(e => e.Email == request.dto.Email, cancellationToken);
            if (isEmailExists)
                throw new InvalidOperationException($"Employee with Email '{request.dto.Email}' already exists.");

           
            bool isNationalIdExists = await _context.Employees.AnyAsync(e => e.NationalId == request.dto.NationalId, cancellationToken);
            if (isNationalIdExists)
                throw new InvalidOperationException($"Employee with National ID '{request.dto.NationalId}' already exists.");

         
            string generatedCode;
            bool codeExists;
            var random = new Random();

            do
            {
               
                var randomNum = random.Next(1000, 10000);
                generatedCode = $"EMP-{randomNum}";

            
                codeExists = await _context.Employees.AnyAsync(e => e.Code == generatedCode, cancellationToken);
            } 
            while (codeExists);

         
            var employee = _mapper.Map<Employee>(request.dto);
            employee.Code = generatedCode;

            _context.Employees.Add(employee);
            await _context.SaveChangesAsync(cancellationToken);

            // 6. جلب الموظف مع تفاصيل القسم لإرجاع الـ Response
            var createdEmployee = await _context.Employees
                .Include(e => e.Department)
                .FirstOrDefaultAsync(e => e.Id == employee.Id, cancellationToken);

            var responseDto = _mapper.Map<EmployeeResponseDto>(createdEmployee);

            return responseDto;
        }
    }
}