using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos
{
	public class LoginDto
	{
		[Required]
		public string PinCode { get; set; } = string.Empty;
	}
}
