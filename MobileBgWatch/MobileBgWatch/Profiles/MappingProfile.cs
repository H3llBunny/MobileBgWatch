using AutoMapper;
using MobileBgWatch.Models;
using MobileBgWatch.ViewModels;

namespace MobileBgWatch.Profiles
{
    public class MappingProfile : Profile
    {
        public MappingProfile()
        {
            CreateMap<Vehicle, VehicleViewModel>();
        }
    }
}
