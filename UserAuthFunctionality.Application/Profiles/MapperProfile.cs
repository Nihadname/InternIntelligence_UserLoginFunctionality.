using AutoMapper;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UserAuthFunctionality.Application.Dtos.Auth;
using UserAuthFunctionality.Core.Entities;

namespace UserAuthFunctionality.Application.Profiles
{
    public class MapperProfile : Profile
    {
        public MapperProfile()
        {
            CreateMap<AppUser, UserGetDto>()
                             .ForMember(s => s.PhoneNumber, map => map.MapFrom(d => d.PhoneNumber));

        }
    }
}
