using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos.Agent
{
    public class CreateAgentDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        [Required]
        [MaxLength(10)]
        public string PinCode { get; set; } = string.Empty;
    }
}
