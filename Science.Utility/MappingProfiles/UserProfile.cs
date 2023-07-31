using AutoMapper;
using Science.Domain.Models;
using Science.DTO.User.Requests;

namespace Science.Utility.MappingProfiles
{
    public class UserProfile : Profile
    {
        public UserProfile() 
        {
            CreateMap<UserRegistrationRequest, User>();
        }
    }
}
