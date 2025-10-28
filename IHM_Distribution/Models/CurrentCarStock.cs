using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IHM_Distribution.Models.Common;

namespace IHM_Distribution.Models
{
	public class CurrentCarStock : Entity
	{
		public Guid ProductId { get; set; }

		[Required]
		[Range(0, int.MaxValue)]
		public int Quantity { get; set; } // The current amount in the car RIGHT NOW.

		// The date this stock was last updated (loaded or sold)
		public DateTime LastUpdated { get; set; } = DateTime.Now;

		// Navigation Property
		[JsonIgnore]
		public Product? Product { get; set; }
	}
}
