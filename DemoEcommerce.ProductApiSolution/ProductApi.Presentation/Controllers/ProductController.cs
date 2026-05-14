using eCommerce.SharedLibrary.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using ProductApi.Application.DTOs;
using ProductApi.Application.DTOs.Conversions;
using ProductApi.Application.Interfaces;

namespace ProductApi.Presentation.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductController : ControllerBase
    {
        private readonly IProduct productInterface;

        public ProductController(IProduct productInterface)
        {
            this.productInterface = productInterface;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductDTO>>> GetProducts()
        {

            // Get all products from the database
            var products = await productInterface.GetAllAsync();

            if (!products.Any())
            {
                return NotFound("No products found.");
            }

            // Convert entities to DTOs
            var (_, list) = ProductConversion.ToDTO(null!, products);

            return list!.Any() ? Ok(list) : NotFound("No products found.");
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<ProductDTO>> GetProductById(int id)
        {
            // Get Single product from the database by ID
            var product = await productInterface.FindByIdAsync(id);

            if (product is null)
            {
                return NotFound($"Product with ID {id} not found.");
            }

            // Convert entity to DTO
            var (productDTO, _) = ProductConversion.ToDTO(product, null!);

            if (productDTO is null)
            {
                return Ok(productDTO);
            }
            else
            {
                return NotFound($"Product with ID {id} not found.");
            }

        }

        [HttpPost]
        public async Task<ActionResult<Response>> CreateProduct(ProductDTO productDTO)
        {
            // Check model state is all data annotations are valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert DTO to entity
            var getEntity = ProductConversion.ToEntity(productDTO);
            var response = await productInterface.CreateAsync(getEntity);

            return response.Success ? Ok(response) : BadRequest(response);

        }

        [HttpPut]
        public async Task<ActionResult<Response>> UpdateProduct(ProductDTO productDTO)
        {
            // Check model state is all data annotations are valid
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Convert DTO to entity
            var getEntity = ProductConversion.ToEntity(productDTO);
            var response = await productInterface.UpdateAsync(getEntity);
            return response.Success ? Ok(response) : BadRequest(response);

        }

        [HttpDelete]
        public async Task<ActionResult<Response>> DeleteProduct(ProductDTO product)
        {
            // convert DTO to entity
            var getEntity = ProductConversion.ToEntity(product);
            var response = await productInterface.DeleteAsync(getEntity);
            return response.Success ? Ok(response) : BadRequest(response);
        }
    }
}

