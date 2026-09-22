using AutoMapper;
using HR_Application.Features.Employees.DTOs;
using HR_Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HR_Application.Features.Employees.Mappings
{
    public class EmployeeMappingProfile : Profile
    {
        public EmployeeMappingProfile()
        {

            CreateMap<CreateEmployeeDto, Employee>();
            CreateMap<UpdateEmployeeDto, Employee>();
            CreateMap<Employee, EmployeeResponseDto>()
                .ForMember(dest => dest.DepartmentName, opt => opt.MapFrom(src => src.Department != null ? src.Department.Name : string.Empty));

        }
    }
}
