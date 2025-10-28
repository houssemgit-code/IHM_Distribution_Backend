using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using IHM_Distribution.Data.Repository;
using IHM_Distribution.Models;
using Microsoft.AspNetCore.Authorization;

namespace IHM_Distribution.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AgentsController : ControllerBase
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ILogger<AgentsController> _logger;

        public AgentsController(IUnitOfWork unitOfWork, ILogger<AgentsController> logger)
        {
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        // GET: api/agents
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Agent>>> GetAgents()
        {
            try
            {
                var agents = await _unitOfWork.Agents.GetAllAsync();
                return Ok(agents);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting all agents");
                return StatusCode(500, "An error occurred while retrieving agents");
            }
        }

        // GET: api/agents/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Agent>> GetAgent(Guid id)
        {
            try
            {
                var agent = await _unitOfWork.Agents.GetByIdAsync(id);

                if (agent == null)
                {
                    return NotFound($"Agent with ID {id} not found");
                }

                return agent;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while getting agent with ID {AgentId}", id);
                return StatusCode(500, "An error occurred while retrieving the agent");
            }
        }

        // POST: api/agents
        [HttpPost]
        public async Task<ActionResult<Agent>> CreateAgent(Agent agent)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                if (string.IsNullOrWhiteSpace(agent.Name))
                {
                    return BadRequest("Agent name is required");
                }

                if (string.IsNullOrWhiteSpace(agent.PinCode) || agent.PinCode.Length != 4)
                {
                    return BadRequest("PIN code must be 4 digits");
                }

                // Check if PIN code is already in use
                var existingAgent = (await _unitOfWork.Agents.FindAsync(a => a.PinCode == agent.PinCode)).FirstOrDefault();
                if (existingAgent != null)
                {
                    return BadRequest("PIN code is already in use");
                }

                await _unitOfWork.Agents.AddAsync(agent);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to create agent");
                }

                return CreatedAtAction(nameof(GetAgent), new { id = agent.Id }, agent);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while creating a new agent");
                return StatusCode(500, "An error occurred while creating the agent");
            }
        }

        // PUT: api/agents/5
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateAgent(Guid id, Agent agent)
        {
            try
            {
                if (id != agent.Id)
                {
                    return BadRequest("Agent ID mismatch");
                }

                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var existingAgent = await _unitOfWork.Agents.GetByIdAsync(id);
                if (existingAgent == null)
                {
                    return NotFound($"Agent with ID {id} not found");
                }

                // Check if new PIN code is already in use by another agent
                if (agent.PinCode != existingAgent.PinCode)
                {
                    var agentWithSamePin = (await _unitOfWork.Agents.FindAsync(a => a.PinCode == agent.PinCode && a.Id != id)).FirstOrDefault();
                    if (agentWithSamePin != null)
                    {
                        return BadRequest("PIN code is already in use by another agent");
                    }
                }

                existingAgent.Name = agent.Name;
                existingAgent.PinCode = agent.PinCode;

                _unitOfWork.Agents.Update(existingAgent);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to update agent");
                }

                return NoContent();
            }
            catch (DbUpdateConcurrencyException ex)
            {
                if (!await AgentExists(id))
                {
                    return NotFound();
                }
                _logger.LogError(ex, "Concurrency error occurred while updating agent with ID {AgentId}", id);
                return StatusCode(500, "A concurrency error occurred while updating the agent");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while updating agent with ID {AgentId}", id);
                return StatusCode(500, "An error occurred while updating the agent");
            }
        }

        // DELETE: api/agents/5
        [Authorize(Roles = "Admin")]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteAgent(Guid id)
        {
            try
            {
                var agent = await _unitOfWork.Agents.GetByIdAsync(id);
                if (agent == null)
                {
                    return NotFound($"Agent with ID {id} not found");
                }

                // Check if agent has related receipts
                var hasReceipts = await _unitOfWork.Receipts.FindAsync(r => r.AgentId == id);
                if (hasReceipts.Any())
                {
                    return BadRequest("Cannot delete agent because they have related receipts. Please delete the receipts first.");
                }

                _unitOfWork.Agents.Remove(agent);
                var saved = await _unitOfWork.CompleteAsync();

                if (!saved)
                {
                    return StatusCode(500, "Failed to delete agent");
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error occurred while deleting agent with ID {AgentId}", id);
                return StatusCode(500, "An error occurred while deleting the agent");
            }
        }

        private async Task<bool> AgentExists(Guid id)
        {
            return await _unitOfWork.Agents.GetByIdAsync(id) != null;
        }
    }
}