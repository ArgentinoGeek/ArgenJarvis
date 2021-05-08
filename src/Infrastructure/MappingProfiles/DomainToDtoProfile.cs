using AutoMapper;
using Core.Country.DTOs;
using Core.State.DTOs;
using Core.TimerMessage.DTOs;
using Domain.Entities;

namespace Infrastructure.MappingProfiles
{
    public class DomainToDtoProfile : Profile
    {
        public DomainToDtoProfile()
        {
            CreateMap<Country, CountryDto>();
            CreateMap<State, StateDto>();
            CreateMap<TimerMessage, TimerMessageDto>();
        }
    }
}
