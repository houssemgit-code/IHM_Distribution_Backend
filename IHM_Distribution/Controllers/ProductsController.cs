using IHM_Distribution.Data.Repository;
using IHM_Distribution.Dtos;
using IHM_Distribution.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace IHM_Distribution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ProductsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ProductsController> _logger;
        private readonly IWebHostEnvironment _environment;

        public ProductsController(IUnitOfWork unitOfWork, ILogger<ProductsController> logger, IWebHostEnvironment environment)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
            _environment = environment;
        }

        // GET: api/products
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            try
            {
                var products = await _unitOfWork.Products.GetAllAsync(includeProperties: "Images");
                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all products");
                return StatusCode(500, "An error occurred while retrieving products");
            }
        }

        // GET: api/products/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(Guid id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id, includeProperties: "Images");

                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                return product;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while retrieving the product");
            }
        }

        // GET: api/products/search?name=product
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Product>>> SearchProducts([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Search term cannot be empty");
                }

                var products = await _unitOfWork.Products.FindAsync(
                    p => p.Name.ToLower().Contains(name.ToLower()) ||
                         (p.Description != null && p.Description.ToLower().Contains(name.ToLower())),
                    includeProperties: "Images");

                return Ok(products);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching products with term {SearchTerm}", name);
                return StatusCode(500, "An error occurred while searching products");
            }
        }

        // POST: api/products
        [HttpPost]
        public async Task<ActionResult<Product>> CreateProduct(Product product)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(product.Name))
                {
                    return BadRequest("Product name is required");
                }

                if (product.Price <= 0)
                {
                    return BadRequest("Price must be greater than 0");
                }

                await _unitOfWork.Products.AddAsync(product);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to create product");
                }

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new product");
                return StatusCode(500, "An error occurred while creating the product");
            }
        }

        // PUT: api/products/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateProduct(Guid id, Product product)
        {
            try
            {
                if (id != product.Id)
                {
                    return BadRequest("Product ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingProduct = await _unitOfWork.Products.GetByIdAsync(id, includeProperties: "Images");
                if (existingProduct == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                // Update properties
                existingProduct.Name = product.Name;
                existingProduct.Description = product.Description;
                existingProduct.Price = product.Price;
                existingProduct.StockInWarehouse = product.StockInWarehouse;

                _unitOfWork.Products.Update(existingProduct);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update product");
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await ProductExists(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error occurred while updating product with ID {ProductId}", id);
                return StatusCode(500, "A concurrency error occurred while updating the product");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while updating the product");
            }
        }

        // DELETE: api/products/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(Guid id)
        {
            try
            {
                var product = await _unitOfWork.Products.GetByIdAsync(id, includeProperties: "Images,ReceiptDetails,LoadedItems,ReturnedItems");
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                // Check if product has related records
                if (product.ReceiptDetails.Any() || product.LoadedItems.Any() || product.ReturnedItems.Any())
                {
                    return BadRequest("Cannot delete product because it has related records. Please delete the related records first.");
                }

                // Delete associated images
                foreach (var image in product.Images.ToList())
                {
                    _unitOfWork.ProductImage?.Remove(image);
                }

                _unitOfWork.Products.Remove(product);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to delete product");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while deleting the product");
            }
        }

        // POST: api/products/5/images
        [HttpPost("{id}/images")]
        public async Task<ActionResult<ProductImage>> AddProductImage(Guid id, [FromForm] ProductImageUploadDto dto)
        {
            try
            {
                var file = dto.File;
                var product = await _unitOfWork.Products.GetByIdAsync(id);
                if (product == null)
                {
                    return NotFound($"Product with ID {id} not found");
                }

                if (file == null || file.Length == 0)
                {
                    return BadRequest("No file uploaded");
                }

                // Validate file type
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                var fileExtension = Path.GetExtension(file.FileName).ToLower();
                if (!allowedExtensions.Contains(fileExtension))
                {
                    return BadRequest("Invalid file type. Only images are allowed.");
                }

                // Validate file size (max 5MB)
                if (file.Length > 5 * 1024 * 1024)
                {
                    return BadRequest("File size too large. Maximum size is 5MB.");
                }

                // Create uploads directory if it doesn't exist
                var uploadsPath = Path.Combine(_environment.WebRootPath, "uploads", "products");
                if (!Directory.Exists(uploadsPath))
                {
                    Directory.CreateDirectory(uploadsPath);
                }

                // Generate unique filename
                var fileName = $"{Guid.NewGuid()}{fileExtension}";
                var filePath = Path.Combine(uploadsPath, fileName);

                // Save file
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                // Create image record
                var image = new ProductImage
                {
                    ProductId = id,
                    ImageUrl = $"/uploads/products/{fileName}",
                    DisplayOrder = product.Images.Count,
                    CreatedAt = DateTime.UtcNow
                };

                await _unitOfWork.ProductImage.AddAsync(image);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    // Delete the uploaded file if database save failed
                    if (System.IO.File.Exists(filePath))
                    {
                        System.IO.File.Delete(filePath);
                    }
                    return StatusCode(500, "Failed to save image record");
                }

                return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, image);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while adding image to product with ID {ProductId}", id);
                return StatusCode(500, "An error occurred while adding the image");
            }
        }

        // DELETE: api/products/images/5
        [HttpDelete("images/{imageId}")]
        public async Task<IActionResult> DeleteProductImage(Guid imageId)
        {
            try
            {
                var image = await _unitOfWork.ProductImage.GetByIdAsync(imageId);
                if (image == null)
                {
                    return NotFound($"Image with ID {imageId} not found");
                }

                // Delete physical file
                var filePath = Path.Combine(_environment.WebRootPath, image.ImageUrl.TrimStart('/'));
                if (System.IO.File.Exists(filePath))
                {
                    System.IO.File.Delete(filePath);
                }

                _unitOfWork.ProductImage.Remove(image);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to delete image");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting image with ID {ImageId}", imageId);
                return StatusCode(500, "An error occurred while deleting the image");
            }
        }

        private async Task<bool> ProductExists(Guid id)
        {
            return await _unitOfWork.Products.GetByIdAsync(id) != null;
        }
    }
}