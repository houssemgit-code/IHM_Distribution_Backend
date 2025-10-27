using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;

namespace IHM_Distribution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CurrentCarStockController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<CurrentCarStockController> _logger;

        public CurrentCarStockController(IUnitOfWork unitOfWork, ILogger<CurrentCarStockController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/currentcarstock
        [HttpGet]
        public async Task<ActionResult<IEnumerable<CurrentCarStock>>> GetCurrentCarStock()
        {
            try
            {
                var stock = await _unitOfWork.CurrentCarStock.GetAllAsync(includeProperties: "Product");
                return Ok(stock);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting current car stock");
                return StatusCode(500, "An error occurred while retrieving current car stock");
            }
        }

        // GET: api/currentcarstock/5
        [HttpGet("{productId}")]
        public async Task<ActionResult<CurrentCarStock>> GetProductCarStock(int productId)
        {
            try
            {
                var stock = await _unitOfWork.CurrentCarStock.GetByIdAsync(productId, includeProperties: "Product");

                if (stock == null)
                {
                    return NotFound($"No car stock found for product with ID {productId}");
                }

                return stock;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting car stock for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while retrieving the product car stock");
            }
        }

        // PUT: api/currentcarstock/5
        [HttpPut("{productId}")]
        public async Task<IActionResult> UpdateProductCarStock(int productId, CurrentCarStock carStock)
        {
            try
            {
                if (productId != carStock.ProductId)
                {
                    return BadRequest("Product ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if product exists
                var product = await _unitOfWork.Products.GetByIdAsync(productId);
                if (product == null)
                {
                    return BadRequest("Product not found");
                }

                var existingStock = await _unitOfWork.CurrentCarStock.GetByIdAsync(productId);
                if (existingStock == null)
                {
                    // Create new stock record
                    carStock.LastUpdated = DateTime.Now;
                    await _unitOfWork.CurrentCarStock.AddAsync(carStock);
                }
                else
                {
                    // Update existing stock
                    existingStock.Quantity = carStock.Quantity;
                    existingStock.LastUpdated = DateTime.Now;
                    _unitOfWork.CurrentCarStock.Update(existingStock);
                }

                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update car stock");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating car stock for product {ProductId}", productId);
                return StatusCode(500, "An error occurred while updating the car stock");
            }
        }

        // POST: api/currentcarstock/bulk-update
        [HttpPost("bulk-update")]
        public async Task<IActionResult> BulkUpdateCarStock(List<CurrentCarStock> carStocks)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                foreach (var carStock in carStocks)
                {
                    var existingStock = await _unitOfWork.CurrentCarStock.GetByIdAsync(carStock.ProductId);
                    if (existingStock == null)
                    {
                        carStock.LastUpdated = DateTime.Now;
                        await _unitOfWork.CurrentCarStock.AddAsync(carStock);
                    }
                    else
                    {
                        existingStock.Quantity = carStock.Quantity;
                        existingStock.LastUpdated = DateTime.Now;
                        _unitOfWork.CurrentCarStock.Update(existingStock);
                    }
                }

                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update car stock");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while bulk updating car stock");
                return StatusCode(500, "An error occurred while updating the car stock");
            }
        }
    }
}