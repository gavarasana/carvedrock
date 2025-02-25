using CarvedRock.Core.Models;
using CarvedRock.Domain.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Headers;
using System.Text.Json;

namespace CarvedRock_Web.Services
{
    public class ProductService(ILogger<ProductService> logger, IHttpClientFactory httpClientFactory, 
                IHttpContextAccessor httpContextAccessor) : IProductService
    {
        // Add a new product
        public async Task<IDictionary<string, string>> AddProductAsAsync(NewProductModel newProduct)
        {
            logger.LogInformation("Creating product. Name: {productName}", newProduct.Name);
            var httpContext = httpContextAccessor.HttpContext;
            var httpClient = httpClientFactory.CreateClient("CarvedRockApi");
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

                if (response.StatusCode == System.Net.HttpStatusCode.BadRequest)
                {
                    var validationErrors = new Dictionary<string, string>();
                    var ignoreErrors = new List<string> { "traceId", "type", "title", "status", "detail", "instance" };
                    foreach (var errorKey in problemDetails.Extensions.Keys.Where(e => ignoreErrors.Contains(e)))
                    {
                        var errorMessages = problemDetails.Extensions[errorKey]!.ToString() ?? string.Empty;
                        validationErrors.Add(errorKey, errorMessages);
                    }
                    return validationErrors;
                }

                var traceId = problemDetails.Extensions["traceId"]!.ToString();

                var fullPath = $"{httpClient.BaseAddress}Product";
                logger.LogWarning("API Failure: {fullPath} Response: {apiResponse}, Trace: {trace}", fullPath, (int)response.StatusCode, traceId);

                throw new Exception(string.Format("API call Failed. {fullPath}", fullPath));
            }

            return new Dictionary<string, string>();
        }

        // Get Product by Id
        public async Task<ProductModel> GetProductByIdAsync(int id)
        {
            logger.LogInformation("Retrieving product by Id. Id: {id}", id);
            var httpContext = httpContextAccessor.HttpContext;
            var httpClient = httpClientFactory.CreateClient("CarvedRockApi");
            if (httpContext != null)
            {
                var accessToken = await httpContext.GetTokenAsync("access_token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await httpClient.GetAsync($"Product/{id}");

            if (!response.IsSuccessStatusCode)
            {
                var fullPath = $"{httpClient.BaseAddress}Product/{id}";
                var content = await response.Content.ReadAsStringAsync();
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content) ?? new ProblemDetails();
                var traceId = problemDetails.Extensions["traceId"]?.ToString();

                logger.LogWarning("Api failure. Path: {fullpath} TraceId: {traceId} Response: {responseCode}", fullPath, traceId, (int)response.StatusCode);

                // Respond with empty product, if no product was found.
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new ProductModel();
                }
                throw new Exception(string.Format("Api failed. Path: {fullpath}", fullPath));
            }

            return await response.Content.ReadFromJsonAsync<ProductModel>() ?? new ProductModel();
        }

        
        // Retrieve products by category.
        public async Task<List<ProductModel>> GetProductsAsAsync(string category)
        {
            
            logger.LogInformation("Retrieving products. Category: {category}", category);
            var httpContext = httpContextAccessor.HttpContext;
            var httpClient = httpClientFactory.CreateClient("CarvedRockApi");
            if (httpContext != null)
            {
                var accessToken = await httpContext.GetTokenAsync("access_token");
                httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
            }

            var response = await httpClient.GetAsync($"Product?category={category}");

            if (!response.IsSuccessStatusCode)
            {
                var fullPath = $"{httpClient.BaseAddress}Product?category={category}";
                var content = await response.Content.ReadAsStringAsync();
                var problemDetails = JsonSerializer.Deserialize<ProblemDetails>(content) ?? new ProblemDetails();
                var traceId = problemDetails.Extensions["traceId"]?.ToString();

                logger.LogWarning("Api failure. Path: {fullpath} TraceId: {traceId} Response: {responseCode}", fullPath, traceId, (int)response.StatusCode);

                throw new Exception(string.Format("Api failed. Path: {fullpath}", fullPath));

            }

            return await response.Content.ReadFromJsonAsync<List<ProductModel>>() ?? [];

        }
    }
}
