using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IHM_Distribution.Models.Common;

namespace IHM_Distribution.Models
{
	public class LoadedItem : Entity
	{
		[Required]
		public Guid DailyTripId { get; set; }

		[Required]
		public Guid ProductId { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Loaded quantity must be at least 1")]
		public int QuantityLoaded { get; set; } // The amount taken from the warehouse

        // Navigation Properties
        [JsonIgnore]
		public DailyTrip? DailyTrip { get; set; }

		[JsonIgnore]
		public Product? Product { get; set; }
	}
}
