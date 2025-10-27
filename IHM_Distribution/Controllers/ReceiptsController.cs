using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;

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
        public async Task<ActionResult<Receipt>> GetReceipt(int id)
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
        public async Task<ActionResult<IEnumerable<Receipt>>> GetAgentReceipts(int agentId)
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
        public async Task<ActionResult<IEnumerable<Receipt>>> GetClientReceipts(int clientId)
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
        public async Task<ActionResult<Receipt>> CreateReceipt(Receipt receipt)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
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

                // Validate receipt details
                if (receipt.ReceiptDetails == null || !receipt.ReceiptDetails.Any())
                {
                    return BadRequest("Receipt must have at least one item");
                }

                // Calculate total amount
                receipt.TotalAmount = receipt.ReceiptDetails.Sum(rd => rd.LineTotal);

                await _unitOfWork.Receipts.AddAsync(receipt);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to create receipt");
                }

                // Reload with related data
                var createdReceipt = await _unitOfWork.Receipts.GetByIdAsync(receipt.Id, includeProperties: "Agent,Client,DailyTrip,ReceiptDetails,ReceiptDetails.Product");

                return CreatedAtAction(nameof(GetReceipt), new { id = receipt.Id }, createdReceipt);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new receipt");
                return StatusCode(500, "An error occurred while creating the receipt");
            }
        }

        // PUT: api/receipts/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateReceipt(int id, Receipt receipt)
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
        public async Task<IActionResult> DeleteReceipt(int id)
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

        private async Task<bool> ReceiptExists(int id)
        {
            return await _unitOfWork.Receipts.GetByIdAsync(id) != null;
        }
    }
}