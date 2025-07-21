using InterBanking.Api.Dtos;
using InterBanking.Api.Models;
using AutoMapper;

namespace InterBanking.Api.Mappers;
public class UserResponseDto_User_Mapper : Profile
{
    public UserResponseDto_User_Mapper()
    {
        base.CreateMap<UserResponseDto, User>()
            .ForMember(dest => dest.Name, config => config.MapFrom(src => src.Name.ToLower()))
            .ForMember(dest => dest.Surname, config => config.MapFrom(src => src.Surname.ToLower()))
            .ForMember(dest => dest.FinCode, config => config.MapFrom(src => src.FinCode.ToLower()))
            .ForMember(dest => dest.ClientCode, config => config.MapFrom(src => src.ClientCode.ToLower()))
            .ForMember(dest => dest.Sid, config => config.MapFrom(src => src.Sid.ToLower()))
            .ForMember(dest => dest.OtpMethod, config => config.MapFrom(src => src.OtpMethod.ToLower()))
            .ForMember(dest => dest.Status, config => config.MapFrom(src => src.Status.ToLower()))
            .ForMember(dest => dest.PinCode, config => config.MapFrom(src => src.PinCode.ToLower()))
            .ForMember(dest => dest.CellPhone, config => config.MapFrom(src => src.CellPhone.ToLower()))
            .ForMember(dest => dest.ClientType, config => config.MapFrom(src => src.ClientType.ToLower()))
            .ForMember(dest => dest.IsLocked, config => config.MapFrom(src => src.IsLocked))
            .ForMember(dest => dest.EMail, config => config.MapFrom(src => src.EMail.ToLower()))
            .ForMember(dest => dest.ExtendedClientCode, config => config.MapFrom(src => src.ExtendedClientCode.ToLower()))
            .ForMember(dest => dest.Mistakes, config => config.MapFrom(src => src.Mistakes))
            .ForMember(dest => dest.Otp, config => config.MapFrom(src => src.Otp.ToLower()));
    }
}