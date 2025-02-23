using CarvedRock.Core.Models;
using CarvedRock.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CarvedRock_Web
{
    public class ProductService(ILogger<ProductService> logger, HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IProductService
    {
        public async Task<ProductModel> CreateProductAsync(NewProductModel newProduct)
        {
            logger.LogInformation("Creating product. Name: {0}", newProduct.Name);
            var httpContext = httpContextAccessor.HttpContext;
            if (httpContext != null)
            {
                var accessToken = await httpContext.GetTokenAsync("access_token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await httpClient.PostAsJsonAsync("Product", newProduct);

            if (!response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content) ?? new ProblemDetails();

            }
        }

        public Task<ProductModel> GetProductByIdAsync(int id)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<ProductModel>> GetProductsForCategoryAsync(string category)
        {
            throw new NotImplementedException();
        }
    }
}
