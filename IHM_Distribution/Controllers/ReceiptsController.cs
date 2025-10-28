using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;
using IHM_Distribution.Dtos;

namespace IHM_Distribution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReceiptsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ReceiptsController> _logger;

        public ReceiptsController(IUnitOfWork unitOfWork, ILogger<ReceiptsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/receipts
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Receipt>>> GetReceipts()
        {
            try
            {
                var receipts = await _unitOfWork.Receipts.GetAllAsync(includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");
                return Ok(receipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all receipts");
                return StatusCode(500, "An error occurred while retrieving receipts");
            }
        }

        // GET: api/receipts/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Receipt>> GetReceipt(Guid id)
        {
            try
            {
                var receipt = await _unitOfWork.Receipts.GetByIdAsync(id, includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");

                if (receipt == null)
                {
                    return NotFound($"Receipt with ID {id} not found");
                }

                return receipt;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting receipt with ID {ReceiptId}", id);
                return StatusCode(500, "An error occurred while retrieving the receipt");
            }
        }

        // GET: api/receipts/agent/5
        [HttpGet("agent/{agentId}")]
        public async Task<ActionResult<IEnumerable<Receipt>>> GetAgentReceipts(Guid agentId)
        {
            try
            {
                var receipts = await _unitOfWork.Receipts.FindAsync(
                    r => r.AgentId == agentId,
                    includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");

                return Ok(receipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting receipts for agent {AgentId}", agentId);
                return StatusCode(500, "An error occurred while retrieving agent receipts");
            }
        }

        // GET: api/receipts/client/5
        [HttpGet("client/{clientId}")]
        public async Task<ActionResult<IEnumerable<Receipt>>> GetClientReceipts(Guid clientId)
        {
            try
            {
                var receipts = await _unitOfWork.Receipts.FindAsync(
                    r => r.ClientId == clientId,
                    includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");

                return Ok(receipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting receipts for client {ClientId}", clientId);
                return StatusCode(500, "An error occurred while retrieving client receipts");
            }
        }

        // POST: api/receipts
        [HttpPost]
        public async Task<ActionResult<Receipt>> CreateReceipt(CreateReceiptDto receiptDto)
        {
            using var transaction = await _unitOfWork.BeginTransactionAsync();

            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate related entities exist
                var dailyTrip = await _unitOfWork.DailyTrips.GetByIdAsync(receiptDto.DailyTripId,
                    includeProperties: "LoadedItems,LoadedItems.Product,ReturnedItems,Receipts,Receipts.ReceiptDetails,Agent");

                if (dailyTrip == null)
                {
                    return BadRequest("Daily trip not found");
                }

                var client = await _unitOfWork.Clients.GetByIdAsync(receiptDto.ClientId);
                if (client == null)
                {
                    return BadRequest("Client not found");
                }

                // Validate receipt details
                if (receiptDto.ReceiptDetails == null || !receiptDto.ReceiptDetails.Any())
                {
                    return BadRequest("Receipt must have at least one item");
                }

                var saleDate = receiptDto.SaleDate;

                if (saleDate.Kind == DateTimeKind.Unspecified)
                {
                    // Treat it as local and convert to UTC
                    saleDate = DateTime.SpecifyKind(saleDate, DateTimeKind.Local).ToUniversalTime();
                }
                else if (saleDate.Kind == DateTimeKind.Local)
                {
                    saleDate = saleDate.ToUniversalTime();
                }

                // Create receipt entity
                var receipt = new Receipt
                {
                    AgentId = dailyTrip.AgentId, // Get AgentId from daily trip
                    ClientId = receiptDto.ClientId,
                    DailyTripId = receiptDto.DailyTripId,
                    SaleDate = saleDate,
                    ReceiptDetails = new List<ReceiptDetail>()
                };

                decimal totalAmount = 0;

                // Process each receipt detail
                foreach (var detailDto in receiptDto.ReceiptDetails)
                {
                    // Calculate line total
                    var lineTotal = detailDto.Quantity * detailDto.UnitPrice;

                    // Create receipt detail
                    var receiptDetail = new ReceiptDetail
                    {
                        ProductId = detailDto.ProductId,
                        Quantity = detailDto.Quantity,
                        UnitPrice = detailDto.UnitPrice,
                        LineTotal = lineTotal
                    };

                    receipt.ReceiptDetails.Add(receiptDetail);
                    totalAmount += lineTotal;

                    // Check if product exists and validate stock availability
                    var product = await _unitOfWork.Products.GetByIdAsync(detailDto.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Product with ID {detailDto.ProductId} not found");
                    }

                    // Check if there's enough stock in the daily trip (loaded items - previous sales)
                    var availableInTrip = await GetAvailableQuantityInTrip(dailyTrip, detailDto.ProductId);

                    if (availableInTrip < detailDto.Quantity)
                    {
                        return BadRequest($"Insufficient stock in daily trip for product {product.Name}. Available: {availableInTrip}, Requested: {detailDto.Quantity}");
                    }
                    // --- UPDATE DAILY TRIP QUANTITY ---
                    var loadedItem = dailyTrip.LoadedItems.FirstOrDefault(li => li.ProductId == detailDto.ProductId && !li.IsDeleted);
                    if (loadedItem != null)
                    {
                        loadedItem.QuantityLoaded -= detailDto.Quantity;

                        if (loadedItem.QuantityLoaded <= 0)
                        {
                            // Mark as deleted instead of removing
                            loadedItem.IsDeleted = true;
                            loadedItem.QuantityLoaded = 0; // optional, keep it at zero
                        }

                        _unitOfWork.LoadedItems.Update(loadedItem);
                    }
                }

                // Set the calculated total amount
                receipt.TotalAmount = totalAmount;

                // Add receipt to database
                await _unitOfWork.Receipts.AddAsync(receipt);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    await transaction.RollbackAsync();
                    return StatusCode(500, "Failed to create receipt");
                }

                await transaction.CommitAsync();

                // Reload with related data
                var createdReceipt = await _unitOfWork.Receipts.GetByIdAsync(receipt.Id,
                    includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");

                return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, createdReceipt);
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error occurred while creating a new receipt");
                return StatusCode(500, "An error occurred while creating the receipt");
            }
        }

        // PUT: api/receipts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReceipt(Guid id, Receipt receipt)
        {
            try
            {
                if (id != receipt.Id)
                {
                    return BadRequest("Receipt ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingReceipt = await _unitOfWork.Receipts.GetByIdAsync(id, includeProperties: "ReceiptDetails");
                if (existingReceipt == null)
                {
                    return NotFound($"Receipt with ID {id} not found");
                }

                // Validate related entities exist
                var agent = await _unitOfWork.Agents.GetByIdAsync(receipt.AgentId);
                if (agent == null)
                {
                    return BadRequest("Agent not found");
                }

                var client = await _unitOfWork.Clients.GetByIdAsync(receipt.ClientId);
                if (client == null)
                {
                    return BadRequest("Client not found");
                }

                var dailyTrip = await _unitOfWork.DailyTrips.GetByIdAsync(receipt.DailyTripId);
                if (dailyTrip == null)
                {
                    return BadRequest("Daily trip not found");
                }

                // Update properties
                existingReceipt.AgentId = receipt.AgentId;
                existingReceipt.ClientId = receipt.ClientId;
                existingReceipt.DailyTripId = receipt.DailyTripId;
                existingReceipt.SaleDate = receipt.SaleDate;
                existingReceipt.TotalAmount = receipt.TotalAmount;

                _unitOfWork.Receipts.Update(existingReceipt);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update receipt");
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await ReceiptExists(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error occurred while updating receipt with ID {ReceiptId}", id);
                return StatusCode(500, "A concurrency error occurred while updating the receipt");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating receipt with ID {ReceiptId}", id);
                return StatusCode(500, "An error occurred while updating the receipt");
            }
        }

        // DELETE: api/receipts/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteReceipt(Guid id)
        {
            try
            {
                var receipt = await _unitOfWork.Receipts.GetByIdAsync(id, includeProperties: "ReceiptDetails");
                if (receipt == null)
                {
                    return NotFound($"Receipt with ID {id} not found");
                }

                _unitOfWork.Receipts.Remove(receipt);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to delete receipt");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting receipt with ID {ReceiptId}", id);
                return StatusCode(500, "An error occurred while deleting the receipt");
            }
        }

        // GET: api/receipts/dailytrip/5
        [HttpGet("dailytrip/{dailyTripId}")]
        public async Task<ActionResult<IEnumerable<Receipt>>> GetDailyTripReceipts(Guid dailyTripId)
        {
            try
            {
                var receipts = await _unitOfWork.Receipts.FindAsync(
                    r => r.DailyTripId == dailyTripId,
                    includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");

                return Ok(receipts);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting receipts for daily trip {DailyTripId}", dailyTripId);
                return StatusCode(500, "An error occurred while retrieving daily trip receipts");
            }
        }

        private async Task<bool> ReceiptExists(Guid id)
        {
            return await _unitOfWork.Receipts.GetByIdAsync(id) != null;
        }

        // Helper method to calculate available quantity in daily trip
        private async Task<int> GetAvailableQuantityInTrip(DailyTrip dailyTrip, Guid productId)
        {
            // Get loaded quantity for this product
            var loadedQuantity = dailyTrip.LoadedItems
                .Where(li => li.ProductId == productId)
                .Sum(li => li.QuantityLoaded);

            // Get returned quantity for this product (if any)
            var returnedQuantity = dailyTrip.ReturnedItems
                .Where(ri => ri.ProductId == productId)
                .Sum(ri => ri.QuantityReturned);

            // Get previously sold quantity for this product in this trip
            var soldQuantity = dailyTrip.Receipts
                .SelectMany(r => r.ReceiptDetails)
                .Where(rd => rd.ProductId == productId)
                .Sum(rd => rd.Quantity);

            // Available = Loaded + Returned - Previously Sold
            return loadedQuantity + returnedQuantity - soldQuantity;
        }

    }
}