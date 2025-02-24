using CarvedRock.Data.DatabaseContext;
using CarvedRock.Data.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CarvedRock.Data.Repositories
{
    public class CarvedRockRepository(CarvedRockContext carvedRockContext, ILogger<CarvedRockRepository> logger) : ICarvedRockRepository
    {
        private static List<string> validCategories = ["kayak", "equip", "boots", "all"];
        public async Task<Product> CreateProductAsync(Product product)
        {
            logger.LogInformation("Saving product {0}", product.Name);
            product.Name = product.Name!.Trim();
            carvedRockContext.Products.Add(product);
            await carvedRockContext.SaveChangesAsync();
            return product;
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            logger.LogInformation("Retrieving product with id: {0}", id);
            return await carvedRockContext.Products.FindAsync(id);
        }

        public async Task<List<Product>> GetProductsAsync(string category)
        {
            logger.LogInformation("Retrieving all products for category {0}", category);
            try
            {
                if (!validCategories.Contains(category))
                {
                    throw new Exception(string.Concat("Simulated exception for category {0}", category));
                }
                if (category == "all")
                {
                    return await carvedRockContext.Products.ToListAsync();
                }
                return await carvedRockContext.Products.Where(p => p.Category == category).ToListAsync();
            }
            catch (Exception ex)
            {
                var newException = new ApplicationException("Something bad happend in database", ex);
                newException.Data.Add("Category", category);
                throw newException;
            }
        }

        public async Task<bool> IsProductNameUniqueAsync(string name, CancellationToken token = default)
        {
            logger.LogInformation("Querying database to check if the product is unique. Name: {0}", name);
            return await carvedRockContext.Products.AllAsync(p => p.Name != name,token);
        }
    }
}
