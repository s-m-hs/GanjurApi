using AutoMapper;
using CY_BM;
using CY_DM;

namespace CY_WebApi
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CyMenu, MenuDTO>().ReverseMap();
            CreateMap<CyMenuItem, MenuItemDTO>().ReverseMap();
            CreateMap<CyCategory, CategoryDTO>().ReverseMap();
            CreateMap<CySubject, SubjectDTO>().ReverseMap();
            CreateMap<CyUser, UserDTO>().ReverseMap();
            CreateMap<CySkin, SkinDTO>().ReverseMap();
            CreateMap<CyAddress, AddressDTO>().ReverseMap();
            CreateMap<CyAddress, AddressPutDTO>().ReverseMap();
            CreateMap<CyKeyData, KeyDataDTO>().ReverseMap();
            CreateMap<CyOrder, OrderDTO>().ReverseMap();
            CreateMap<CyOrderItem, OrderItemDTO>().ReverseMap();
            CreateMap<CyOrder, OrderBDTO>().ReverseMap();
            CreateMap<CyOrderItem, OrderItemByIdDTO>().ReverseMap();
            CreateMap<CyOrderMessage, OrderMessageDTO>().ReverseMap();
            CreateMap<CyProduct, ProductDTO>().ReverseMap();
            CreateMap<CyManufacturer, ManufacturerDTO>().ReverseMap();
            CreateMap<CyProductSpec, ProductSpecDTO>().ReverseMap();
            CreateMap<CyProfile, ProfileDTO>().ReverseMap();
            CreateMap<CyInspectionItem, InspectionItemDTO>().ReverseMap();
            CreateMap<CyProductCategory, ProductCategoryDTO>().ReverseMap();
            CreateMap<CyPcbForm, pcbDTO>().ReverseMap();
            CreateMap<CyService, ServiceDTO>().ReverseMap();
            CreateMap<CouponDTO, CyCoupon>().ReverseMap();
            CreateMap<CouponUsageDTO, CyCouponUsage>().ReverseMap();
            CreateMap<TaskDTO, CyTask>().ReverseMap();
            CreateMap<Account, AccountDTO>().ReverseMap();
            CreateMap<Voucher, VoucherDTO>().ReverseMap();
            CreateMap<VoucherItem, VoucherItemDTO>().ReverseMap();
            CreateMap<Voucher, VoucherDTOB>().ReverseMap();
            CreateMap<VoucherItem, VoucherItemDTOB>().ReverseMap();




            CreateMap<CyInspectionForm, InspectionFormDTO>()
                .ForMember(dest => dest.Items, opt => opt.MapFrom(src => src.Items)).ReverseMap();


        }
    }
}
