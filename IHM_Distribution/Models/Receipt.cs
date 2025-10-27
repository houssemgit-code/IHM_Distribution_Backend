using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IHM_Distribution.Models
{
	public class Receipt
	{
		public int Id { get; set; }

		[Required]
		public int AgentId { get; set; }

		[Required]
		public int ClientId { get; set; }

		[Required]
		public DateTime SaleDate { get; set; } = DateTime.Now;

		[Required]
		[Precision(18, 2)]
		public decimal TotalAmount { get; set; }

		[Required]
		public int DailyTripId { get; set; }

		// Navigation Properties
		[JsonIgnore]
		public Agent? Agent { get; set; }

		[JsonIgnore]
		public Client? Client { get; set; }


		[JsonIgnore]
		public DailyTrip? DailyTrip { get; set; }

		public ICollection<ReceiptDetail> ReceiptDetails { get; set; } = new List<ReceiptDetail>();
	}
}
