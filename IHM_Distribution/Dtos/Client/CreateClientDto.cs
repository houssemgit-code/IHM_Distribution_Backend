using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos.Client
{
    public class CreateClientDto
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
    }
}
