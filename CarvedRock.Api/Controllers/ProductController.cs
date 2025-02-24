using CarvedRock.Core.Models;
using CarvedRock.Domain.Services;
using CarvedRock.Domain.Validators;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace CarvedRock.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController(ILogger<ProductController> logger, IProductService productService,
            NewProductValidator newProductValidator, IWebHostEnvironment hostEnvironment) : ControllerBase
    {

        [HttpGet()]
        [AllowAnonymous]
        public async Task<ActionResult<IEnumerable<ProductModel>>> GetAll([FromQuery(Name = "Category")] string category = "all")
        {
            logger.LogInformation("Retrieving all products");
            var products = await productService.GetProductsForCategoryAsync(category);
            return Ok(products);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(ProductModel), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetProductById(int id)
        {
            logger.LogInformation("Retrieving product by id {0}", id);
            var product = await productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            return Ok(product);
        }

        [HttpPost]
        [ProducesResponseType<ProductModel>(StatusCodes.Status201Created)]
        [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateProduct([FromBody] NewProductModel product)
        {
            logger.LogInformation("Validating product properties.");
            var validationResult = await newProductValidator.ValidateAsync(product);
            if (!validationResult.IsValid)
            {
                return BadRequest(validationResult.Errors);
            }
            var newProduct = await productService.CreateProductAsync(product);
            return CreatedAtRoute("GetProductById", new { id = newProduct.Id }, newProduct);
        }
    }
}
