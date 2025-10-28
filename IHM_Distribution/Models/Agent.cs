using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IHM_Distribution.Models.Common;

namespace IHM_Distribution.Models
{
	public class Agent : Entity
	{
		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

        [Required]
        public string UserEmail { get; set; } = string.Empty;

        public string Role { get; set; } = "Agent"; // default role

        [Required]
		[MaxLength(10)]
		public string PinCode { get; set; } = string.Empty;

		// Navigation Property
		[JsonIgnore]
		public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
	}
}
