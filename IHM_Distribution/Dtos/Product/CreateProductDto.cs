using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos.Product
{
    public class CreateProductDto
    {
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal Price { get; set; }

        public int StockInWarehouse { get; set; } = 0;
    }
}
