using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using IHM_Distribution.Models.Common;

namespace IHM_Distribution.Models
{
	public class Client : Entity
	{
		[Required]
		[MaxLength(200)]
		public string ShopName { get; set; } = string.Empty;

		[Required]
		[MaxLength(100)]
		public string OwnerName { get; set; } = string.Empty;

		[MaxLength(300)]
		public string? Address { get; set; }

		[EmailAddress]
		[MaxLength(150)]
		public string? Email { get; set; }

		[Phone]
		[MaxLength(20)]
		public string? PhoneNumber { get; set; }

		[Phone]
		[MaxLength(20)]
		public string? MobileNumber { get; set; }

		public double? Latitude { get; set; }
		public double? Longitude { get; set; }

		// Navigation Property
		[JsonIgnore]
		public ICollection<Receipt> Receipts { get; set; } = new List<Receipt>();
	}
}
