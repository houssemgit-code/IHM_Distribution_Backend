
namespace IHM_Distribution.Dtos.DailyTrips
{
    public class ReturnedItemDto
    {
        public Guid ProductId { get; internal set; }
        public string? ProductName { get; internal set; }
        public int QuantityReturned { get; internal set; }
    }
}
