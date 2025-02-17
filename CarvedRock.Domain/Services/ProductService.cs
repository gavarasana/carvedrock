using AutoMapper;
using CarvedRock.Core.Models;
using CarvedRock.Data.Entities;
using CarvedRock.Data.Repositories;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarvedRock.Domain.Services
{
    public class ProductService(ICarvedRockRepository carvedRockRepository,
            IMapper mapper, ILogger<ProductService> logger) : IProductService
    {
    

        public async Task<ProductModel> CreateProductAsync(NewProductModel newProduct)
        {
            logger.LogInformation("Creating product. Name:{0}, Category:{1}", newProduct.Name, newProduct.Category);
            var product = mapper.Map<Product>(newProduct);
            var createdProduct = await carvedRockRepository.CreateProductAsync(product);
            return mapper.Map<ProductModel>(createdProduct);

        }

        public async Task<ProductModel> GetProductByIdAsync(int id)
        {
            var product = await carvedRockRepository.GetProductByIdAsync(id);
            return mapper.Map<ProductModel>(product);
        }

        public async Task<IEnumerable<ProductModel>> GetProductsForCategoryAsync(string category)
        {
           var results = await carvedRockRepository.GetProductsAsync(category);
            return mapper.Map<IEnumerable<ProductModel>>(results);
        }
    }
}
