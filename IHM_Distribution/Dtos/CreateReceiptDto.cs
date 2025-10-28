using System.ComponentModel.DataAnnotations;

namespace IHM_Distribution.Dtos
{
    public class CreateReceiptDto
    {
        [Required]
        public Guid DailyTripId { get; set; }

        [Required]
        public Guid ClientId { get; set; }

        public DateTime SaleDate { get; set; } = DateTime.Now;

        public List<ReceiptDetailDto> ReceiptDetails { get; set; } = new List<ReceiptDetailDto>();
    }

}
