using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace IHM_Distribution.Models
{
	public class Product
	{
		public int Id { get; set; }

		[Required]
		[MaxLength(100)]
		public string Name { get; set; } = string.Empty;

		[MaxLength(500)]
		public string? Description { get; set; }

		[Required]
		[Precision(18, 2)]
		public decimal Price { get; set; }

		public int StockInWarehouse { get; set; } = 0;

        public ICollection<ProductImage> Images { get; set; } = new List<ProductImage>();


        // Navigation Properties
        [JsonIgnore]
		public ICollection<ReceiptDetail> ReceiptDetails { get; set; } = new List<ReceiptDetail>();

		// REPLACED: CarStocks with LoadedItems and ReturnedItems
		[JsonIgnore]
		public ICollection<LoadedItem> LoadedItems { get; set; } = new List<LoadedItem>();

		[JsonIgnore]
		public ICollection<ReturnedItem> ReturnedItems { get; set; } = new List<ReturnedItem>();
	}
}
