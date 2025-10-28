using IHM_Distribution.Models.Common;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IHM_Distribution.Models
{
	public class ReceiptDetail : Entity
	{
		[Required]
		public Guid ReceiptId { get; set; }

		[Required]
		public Guid ProductId { get; set; }

		[Required]
		[Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
		public int Quantity { get; set; }

		[Required]
		[Precision(18, 2)]
		public decimal UnitPrice { get; set; }

		[Required]
		[Precision(18, 2)]
		public decimal LineTotal { get; set; }

		// Navigation Properties
		[JsonIgnore]
		public Receipt? Receipt { get; set; }

		[JsonIgnore]
		public Product? Product { get; set; }
	}
}
