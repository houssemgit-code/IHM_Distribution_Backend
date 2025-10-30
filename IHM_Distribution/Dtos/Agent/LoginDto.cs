using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos.Agent
{
	public class LoginDto
	{
		[Required]
		public string PinCode { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;
    }
}
