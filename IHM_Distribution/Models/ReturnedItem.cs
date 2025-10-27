using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IHM_Distribution.Models
{
	public class ReturnedItem
	{
		public int Id { get; set; }

		[Required]
		public int DailyTripId { get; set; }

		[Required]
		public int ProductId { get; set; }

		[Required]
		[Range(0, int.MaxValue)] // Can return 0 if everything was sold, but usually >0
		public int QuantityReturned { get; set; } // The amount put back into the warehouse

		// Navigation Properties
		[JsonIgnore]
		public DailyTrip? DailyTrip { get; set; }

		[JsonIgnore]
		public Product? Product { get; set; }
	}
}
