using CarvedRock.Core.Models;

namespace CarvedRock_Web.Services
{
    public interface IProductService
    {
        Task<List<ProductModel>> GetProductsAsAsync(string category = "all");
        Task<ProductModel> GetProductById(int Id);
        Task<IDictionary<string, string>> AddProductAsAsync(NewProductModel newProductModel);
    }
}
