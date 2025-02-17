using CarvedRock.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarvedRock.Domain.Services
{
    public interface IProductService
    {
        Task<IEnumerable<ProductModel>> GetProductsForCategoryAsync(string category);
        Task<ProductModel> GetProductByIdAsync(int id);
        Task<ProductModel> CreateProductAsync(NewProductModel product);
    }
}
