using CarvedRock.Data.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarvedRock.Data.Repositories
{
    public interface ICarvedRockRepository
    {
        Task<List<Product>> GetProductsAsync(string category);
        Task<Product?> GetProductByIdAsync(int id);
        Task<bool> IsProductNameUniqueAsync(string name, CancellationToken token);
        Task<Product> CreateProductAsync(Product product);
    }
}
