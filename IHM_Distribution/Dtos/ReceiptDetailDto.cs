using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos
{
    public class ReceiptDetailDto
    {
        [Required]
        public Guid ProductId { get; set; }

        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        public int Quantity { get; set; }

        [Required]
        [Precision(18, 2)]
        public decimal UnitPrice { get; set; }
    }
}
