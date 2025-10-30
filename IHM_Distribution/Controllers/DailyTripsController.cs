using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;
using IHM_Distribution.Dtos;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace IHM_Distribution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DailyTripsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<DailyTripsController> _logger;

        public DailyTripsController(IUnitOfWork unitOfWork, ILogger<DailyTripsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/dailytrips
        [HttpGet]
        public async Task<ActionResult<IEnumerable<DailyTrip>>> GetDailyTrips()
        {
            try
            {
                var trips = await _unitOfWork.DailyTrips.GetAllAsync(includeProperties: "Agent,LoadedItems,ReturnedItems,Receipts");
                // Filter out deleted loaded items
                foreach (var trip in trips)
                {
                    trip.LoadedItems = trip.LoadedItems.Where(li => !li.IsDeleted).ToList();
                }
                return Ok(trips);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all daily trips");
                return StatusCode(500, "An error occurred while retrieving daily trips");
            }
        }

        // GET: api/dailytrips/5
        [HttpGet("{id}")]
        public async Task<ActionResult<DailyTrip>> GetDailyTrip(Guid id)
        {
            try
            {
                var trip = await _unitOfWork.DailyTrips.GetByIdAsync(id, includeProperties: "Agent,LoadedItems,ReturnedItems,Receipts,Receipts.Client,Receipts.ReceiptDetails,Receipts.ReceiptDetails.Product");

                if (trip == null)
                {
                    return NotFound($"Daily trip with ID {id} not found");
                }

                return trip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting daily trip with ID {TripId}", id);
                return StatusCode(500, "An error occurred while retrieving the daily trip");
            }
        }

        // GET: api/dailytrips/agent/5?date=2023-12-01
        [HttpGet("agent/{agentId}")]
        public async Task<ActionResult<DailyTrip>> GetAgentDailyTrip(Guid agentId, [FromQuery] DateTime? date = null)
        {
            try
            {
                var tripDate = (date ?? DateTime.UtcNow.Date).ToUniversalTime();
                var nextDay = tripDate.AddDays(1);

                var trip = (await _unitOfWork.DailyTrips.FindAsync(
                    t => t.AgentId == agentId && t.Date >= tripDate && t.Date < nextDay,
                    includeProperties: "Agent,LoadedItems,ReturnedItems,Receipts,Receipts.Client,Receipts.ReceiptDetails,Receipts.ReceiptDetails.Product"))
                    .FirstOrDefault();

                if (trip == null)
                    return Ok($"No daily trip found for agent {agentId} on {tripDate:yyyy-MM-dd}");
                trip.LoadedItems = trip.LoadedItems.Where(li => !li.IsDeleted).ToList();

                return trip;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting daily trip for agent {AgentId} on {Date}", agentId, date);
                return StatusCode(500, "An error occurred while retrieving the daily trip");
            }
        }


        // POST: api/dailytrips
        [HttpPost]
        public async Task<ActionResult<DailyTrip>> CreateDailyTrip(DailyTrip dailyTrip)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if agent exists
                var agent = await _unitOfWork.Agents.GetByIdAsync(dailyTrip.AgentId);
                if (agent == null)
                {
                    return BadRequest("Agent not found");
                }

                // Check if trip already exists for this agent and date
                var existingTrip = (await _unitOfWork.DailyTrips.FindAsync(
                    t => t.AgentId == dailyTrip.AgentId && t.Date.Date == dailyTrip.Date.Date))
                    .FirstOrDefault();

                if (existingTrip != null)
                {
                    return BadRequest("A daily trip already exists for this agent and date");
                }

                await _unitOfWork.DailyTrips.AddAsync(dailyTrip);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to create daily trip");
                }

                return CreatedAtAction(nameof(GetDailyTrip), new { id = dailyTrip.Id }, dailyTrip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new daily trip");
                return StatusCode(500, "An error occurred while creating the daily trip");
            }
        }

        // PUT: api/dailytrips/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDailyTrip(Guid id, DailyTrip dailyTrip)
        {
            try
            {
                if (id != dailyTrip.Id)
                {
                    return BadRequest("Daily trip ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingTrip = await _unitOfWork.DailyTrips.GetByIdAsync(id);
                if (existingTrip == null)
                {
                    return NotFound($"Daily trip with ID {id} not found");
                }

                // Check if agent exists
                var agent = await _unitOfWork.Agents.GetByIdAsync(dailyTrip.AgentId);
                if (agent == null)
                {
                    return BadRequest("Agent not found");
                }

                existingTrip.Date = dailyTrip.Date;
                existingTrip.AgentId = dailyTrip.AgentId;

                _unitOfWork.DailyTrips.Update(existingTrip);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update daily trip");
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await DailyTripExists(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error occurred while updating daily trip with ID {TripId}", id);
                return StatusCode(500, "A concurrency error occurred while updating the daily trip");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating daily trip with ID {TripId}", id);
                return StatusCode(500, "An error occurred while updating the daily trip");
            }
        }

        // DELETE: api/dailytrips/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDailyTrip(Guid id)
        {
            try
            {
                var trip = await _unitOfWork.DailyTrips.GetByIdAsync(id, includeProperties: "LoadedItems,ReturnedItems,Receipts");
                if (trip == null)
                {
                    return NotFound($"Daily trip with ID {id} not found");
                }

                // Check if trip has related records
                if (trip.LoadedItems.Any() || trip.ReturnedItems.Any() || trip.Receipts.Any())
                {
                    return BadRequest("Cannot delete daily trip because it has related records. Please delete the related records first.");
                }

                _unitOfWork.DailyTrips.Remove(trip);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to delete daily trip");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting daily trip with ID {TripId}", id);
                return StatusCode(500, "An error occurred while deleting the daily trip");
            }
        }

        // POST: api/dailytrips/start
        [HttpPost("start")]
        public async Task<ActionResult<DailyTrip>> StartDailyTrip(StartTripRequestDto request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Check if agent exists
                var agent = await _unitOfWork.Agents.GetByIdAsync(request.AgentId);
                if (agent == null)
                {
                    return BadRequest("Agent not found");
                }

                // Check if trip already exists for this agent today
                var tripDate = (DateTime.UtcNow.Date).ToUniversalTime();
                var nextDay = tripDate.AddDays(1); var existingTrip = (await _unitOfWork.DailyTrips.FindAsync(
                    t => t.AgentId == request.AgentId && t.Date >= tripDate && t.Date < nextDay))
                    .FirstOrDefault();

                if (existingTrip != null)
                {
                    return BadRequest("A daily trip already exists for this agent today");
                }

                // Check for returned items from the last trip
                var lastTrip = (await _unitOfWork.DailyTrips.FindAsync(
                    t => t.AgentId == request.AgentId && t.Date >= tripDate && t.Date < nextDay,
                    includeProperties: "ReturnedItems,ReturnedItems.Product"))
                    .OrderByDescending(t => t.Date)
                    .FirstOrDefault();

                // Create new daily trip
                var dailyTrip = new DailyTrip
                {
                    Date = tripDate,
                    AgentId = request.AgentId,
                    LoadedItems = new List<LoadedItem>()
                };

                // Process loaded items and update warehouse stock
                foreach (var itemRequest in request.LoadedItems)
                {
                    var product = await _unitOfWork.Products.GetByIdAsync(itemRequest.ProductId);
                    if (product == null)
                    {
                        return BadRequest($"Product with ID {itemRequest.ProductId} not found");
                    }

                    // Check if we have enough stock
                    if (product.StockInWarehouse < itemRequest.Quantity)
                    {
                        return BadRequest($"Insufficient stock for product {product.Name}. Available: {product.StockInWarehouse}, Requested: {itemRequest.Quantity}");
                    }

                    // Check if this product was returned in the last trip
                    var returnedQuantity = 0;
                    if (lastTrip != null)
                    {
                        var returnedItem = lastTrip.ReturnedItems.FirstOrDefault(ri => ri.ProductId == itemRequest.ProductId);
                        if (returnedItem != null)
                        {
                            returnedQuantity = returnedItem.QuantityReturned;
                            _logger.LogInformation($"Found {returnedQuantity} returned items of product {product.Name} from last trip");
                        }
                    }

                    // Calculate actual quantity to load (requested - returned from last trip)
                    var actualQuantityToLoad = Math.Max(0, itemRequest.Quantity - returnedQuantity);

                    if (actualQuantityToLoad > 0)
                    {
                        // Update warehouse stock
                        product.StockInWarehouse -= actualQuantityToLoad;

                        // Add to loaded items
                        dailyTrip.LoadedItems.Add(new LoadedItem
                        {
                            ProductId = itemRequest.ProductId,
                            QuantityLoaded = actualQuantityToLoad
                        });
                    }

                    // If there were returned items, they're automatically considered as loaded
                    // without taking from warehouse stock
                    if (returnedQuantity > 0)
                    {
                        _logger.LogInformation($"{returnedQuantity} items of {product.Name} from previous returns are automatically loaded");
                    }
                }

                await _unitOfWork.DailyTrips.AddAsync(dailyTrip);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to start daily trip");
                }

                // Reload the trip with related data
                var createdTrip = await _unitOfWork.DailyTrips.GetByIdAsync(dailyTrip.Id,
                    includeProperties: "Agent,LoadedItems,LoadedItems.Product,ReturnedItems,Receipts");

                return CreatedAtAction(nameof(GetDailyTrip), new { id = dailyTrip.Id }, createdTrip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while starting daily trip for agent {AgentId}", request.AgentId);
                return StatusCode(500, "An error occurred while starting the daily trip");
            }
        }

        // POST: api/dailytrips/end/5
        [HttpPost("end/{id}")]
        public async Task<ActionResult<DailyTrip>> EndDailyTrip(Guid id)
        {
            try
            {
                var trip = await _unitOfWork.DailyTrips.GetByIdAsync(id,
                    includeProperties: "LoadedItems,LoadedItems.Product,ReturnedItems,ReturnedItems.Product,Receipts,Receipts.ReceiptDetails");

                if (trip == null)
                {
                    return NotFound($"Daily trip with ID {id} not found");
                }

                // Calculate sold quantities from receipts
                var soldQuantities = trip.Receipts
                    .SelectMany(r => r.ReceiptDetails)
                    .GroupBy(rd => rd.ProductId)
                    .Select(g => new
                    {
                        ProductId = g.Key,
                        QuantitySold = g.Sum(rd => rd.Quantity)
                    })
                    .ToDictionary(x => x.ProductId, x => x.QuantitySold);

                // Process each loaded item to calculate returns
                foreach (var loadedItem in trip.LoadedItems)
                {
                    var quantitySold = soldQuantities.ContainsKey(loadedItem.ProductId)
                        ? soldQuantities[loadedItem.ProductId]
                        : 0;

                    var quantityReturned = loadedItem.QuantityLoaded - quantitySold;

                    if (quantityReturned > 0)
                    {
                        // Check if returned item already exists for this product
                        var existingReturn = trip.ReturnedItems.FirstOrDefault(ri => ri.ProductId == loadedItem.ProductId);

                        if (existingReturn != null)
                        {
                            existingReturn.QuantityReturned = quantityReturned;
                        }
                        else
                        {
                            trip.ReturnedItems.Add(new ReturnedItem
                            {
                                ProductId = loadedItem.ProductId,
                                QuantityReturned = quantityReturned
                            });
                        }
                    }
                    else
                    {
                        // Remove any existing return if quantity returned is 0 or negative
                        var existingReturn = trip.ReturnedItems.FirstOrDefault(ri => ri.ProductId == loadedItem.ProductId);
                        if (existingReturn != null)
                        {
                            trip.ReturnedItems.Remove(existingReturn);
                        }
                    }
                }

                _unitOfWork.DailyTrips.Update(trip);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to end daily trip");
                }

                // Reload the trip with updated data
                var updatedTrip = await _unitOfWork.DailyTrips.GetByIdAsync(id,
                    includeProperties: "Agent,LoadedItems,LoadedItems.Product,ReturnedItems,ReturnedItems.Product,Receipts");

                return Ok(updatedTrip);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while ending daily trip with ID {TripId}", id);
                return StatusCode(500, "An error occurred while ending the daily trip");
            }
        }

        // POST: api/dailytrips/empty-car
        [HttpPost("empty-car")]
        public async Task<ActionResult> EmptyCar(EmptyCarRequestDto request)
        {
            try
            {
                var trip = await _unitOfWork.DailyTrips.GetByIdAsync(request.DailyTripId,
                    includeProperties: "ReturnedItems,ReturnedItems.Product");

                if (trip == null)
                {
                    return NotFound($"Daily trip with ID {request.DailyTripId} not found");
                }

                if (!trip.ReturnedItems.Any())
                {
                    return Ok("No returned items to empty");
                }

                // Add returned items back to warehouse stock
                foreach (var returnedItem in trip.ReturnedItems)
                {
                    if (returnedItem.Product != null)
                    {
                        returnedItem.Product.StockInWarehouse += returnedItem.QuantityReturned;
                    }
                    else
                    {
                        // If product is not loaded, get it from repository
                        var product = await _unitOfWork.Products.GetByIdAsync(returnedItem.ProductId);
                        if (product != null)
                        {
                            product.StockInWarehouse += returnedItem.QuantityReturned;
                        }
                    }
                }

                // Clear returned items
                trip.ReturnedItems.Clear();

                _unitOfWork.DailyTrips.Update(trip);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to empty car");
                }

                return Ok("Car emptied successfully. Returned items have been added back to warehouse stock.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while emptying car for daily trip with ID {TripId}", request.DailyTripId);
                return StatusCode(500, "An error occurred while emptying the car");
            }
        }

        private async Task<bool> DailyTripExists(Guid id)
        {
            return await _unitOfWork.DailyTrips.GetByIdAsync(id) != null;
        }
    }
}