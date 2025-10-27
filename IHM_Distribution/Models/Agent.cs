using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IHM_Distribution.Models
{
	public class Agent
	{
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[Required]
		[MaxLength(10)]
		public string PinCode { get; set; } = string.Empty;

		// Navigation Property
		[JsonIgnore]
		public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
	}
}
