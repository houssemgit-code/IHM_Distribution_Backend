using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IHM_Distribution.Models
{
	public class DailyTrip
	{
		public int Id { get; set; }

		[Required]
		[DataType(DataType.Date)]
		public DateTime Date { get; set; } = DateTime.Today;

		[Required]
		public int AgentId { get; set; }

		// Navigation Properties
		[JsonIgnore]
		public Agent? Agent { get; set; }

		// A trip has a collection of loaded items, sold items (via receipts), and returned items.
		public ICollection<LoadedItem> LoadedItems { get; set; } = new List<LoadedItem>();
		public ICollection<ReturnedItem> ReturnedItems { get; set; } = new List<ReturnedItem>();

		// This is derived from the Receipts linked to this agent and date.
		// It's not stored in the database but can be calculated.
		[NotMapped]
		public decimal TotalSales => Receipts?.Sum(r => r.TotalAmount) ?? 0;

		[JsonIgnore]
		public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
	}
}
