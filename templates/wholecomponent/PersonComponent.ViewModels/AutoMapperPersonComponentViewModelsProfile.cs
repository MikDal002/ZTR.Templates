using AlDe.MobileApp.ViewModels;
using AutoMapper;
using PersonComponent.Core;

namespace PersonComponent.ViewModels
{
    internal class AutoMapperPersonComponentViewModelsProfile : Profile
    {
        public AutoMapperPersonComponentViewModelsProfile()
        {
            // CreateMap<PersonModel, PersonSearchViewModel>()
            //     .ForMember(d => d.BirthDate, opt => opt.MapFrom(src => src.BirthDate.ToString("d")))
            //     .ForMember(d => d.SortingPoints, opt => opt.Ignore());
        }
    }
}
