using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;
using IHM_Distribution.Dtos.Client;

namespace IHM_Distribution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ClientsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<ClientsController> _logger;

        public ClientsController(IUnitOfWork unitOfWork, ILogger<ClientsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/clients
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Client>>> GetClients()
        {
            try
            {
                var clients = await _unitOfWork.Clients.GetAllAsync();
                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all clients");
                return StatusCode(500, "An error occurred while retrieving clients");
            }
        }

        // GET: api/clients/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Client>> GetClient(Guid id)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(id);

                if (client == null)
                {
                    return NotFound($"Client with ID {id} not found");
                }

                return client;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting client with ID {ClientId}", id);
                return StatusCode(500, "An error occurred while retrieving the client");
            }
        }

        // GET: api/clients/search?name=shop
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<Client>>> SearchClients([FromQuery] string name)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(name))
                {
                    return BadRequest("Search term cannot be empty");
                }

                var clients = await _unitOfWork.Clients.FindAsync(c =>
                    c.ShopName.ToLower().Contains(name.ToLower()) ||
                    c.OwnerName.ToLower().Contains(name.ToLower()));

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching clients with term {SearchTerm}", name);
                return StatusCode(500, "An error occurred while searching clients");
            }
        }

        // GET: api/clients/search/phone?phone=0612345678
        [HttpGet("search/phone")]
        public async Task<ActionResult<IEnumerable<Client>>> SearchClientsByPhone([FromQuery] string phone)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(phone))
                {
                    return BadRequest("Phone number cannot be empty");
                }

                var clients = await _unitOfWork.Clients.FindAsync(c =>
                    (c.PhoneNumber != null && c.PhoneNumber.Contains(phone)) ||
                    (c.MobileNumber != null && c.MobileNumber.Contains(phone)));

                return Ok(clients);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while searching clients by phone {PhoneNumber}", phone);
                return StatusCode(500, "An error occurred while searching clients by phone");
            }
        }

        // POST: api/clients
        [HttpPost]
        public async Task<ActionResult<Client>> CreateClient(CreateClientDto clientToAdd)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Validate required fields
                if (string.IsNullOrWhiteSpace(clientToAdd.ShopName))
                {
                    return BadRequest("Shop name is required");
                }

                if (string.IsNullOrWhiteSpace(clientToAdd.OwnerName))
                {
                    return BadRequest("Owner name is required");
                }
                var client = new Client()
                {
                    ShopName = clientToAdd.ShopName,
                    OwnerName = clientToAdd.OwnerName,
                    Address = clientToAdd.Address,
                    Email = clientToAdd.Email,
                    Latitude = clientToAdd.Latitude,
                    Longitude = clientToAdd.Longitude,
                    MobileNumber = clientToAdd.MobileNumber,
                    PhoneNumber = clientToAdd.PhoneNumber,
                };
                await _unitOfWork.Clients.AddAsync(client);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to create client");
                }

                return CreatedAtAction(nameof(GetClient), new { id = client.Id }, client);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new client");
                return StatusCode(500, "An error occurred while creating the client");
            }
        }

        // PUT: api/clients/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateClient(Guid id, Client client)
        {
            try
            {
                if (id != client.Id)
                {
                    return BadRequest("Client ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingClient = await _unitOfWork.Clients.GetByIdAsync(id);
                if (existingClient == null)
                {
                    return NotFound($"Client with ID {id} not found");
                }

                // Update properties
                existingClient.ShopName = client.ShopName;
                existingClient.OwnerName = client.OwnerName;
                existingClient.Address = client.Address;
                existingClient.Email = client.Email;
                existingClient.PhoneNumber = client.PhoneNumber;
                existingClient.MobileNumber = client.MobileNumber;
                existingClient.Latitude = client.Latitude;
                existingClient.Longitude = client.Longitude;

                _unitOfWork.Clients.Update(existingClient);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update client");
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await ClientExists(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error occurred while updating client with ID {ClientId}", id);
                return StatusCode(500, "A concurrency error occurred while updating the client");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating client with ID {ClientId}", id);
                return StatusCode(500, "An error occurred while updating the client");
            }
        }

        // DELETE: api/clients/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteClient(Guid id)
        {
            try
            {
                var client = await _unitOfWork.Clients.GetByIdAsync(id);
                if (client == null)
                {
                    return NotFound($"Client with ID {id} not found");
                }

                // Check if client has related receipts
                var hasReceipts = await _unitOfWork.Receipts.FindAsync(r => r.ClientId == id);
                if (hasReceipts.Any())
                {
                    return BadRequest("Cannot delete client because they have related receipts. Please delete the receipts first.");
                }

                _unitOfWork.Clients.Remove(client);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to delete client");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting client with ID {ClientId}", id);
                return StatusCode(500, "An error occurred while deleting the client");
            }
        }

        private async Task<bool> ClientExists(Guid id)
        {
            return await _unitOfWork.Clients.GetByIdAsync(id) != null;
        }
    }
}