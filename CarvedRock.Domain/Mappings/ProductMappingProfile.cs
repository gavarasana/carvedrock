using AutoMapper;
using CarvedRock.Core.Models;
using CarvedRock.Data.Entities;

namespace CarvedRock.Domain.Mappings
{
    public class ProductMappingProfile : Profile
    {
        public ProductMappingProfile()
        {
            CreateMap<Product, ProductModel>()
                .ReverseMap();

            CreateMap<NewProductModel, Product>();
        }
    }
}
