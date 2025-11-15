namespace IHM_Distribution.Dtos.DailyTrips
{
    public class StartTripRequestDto
    {
        public Guid AgentId { get; set; }
        public List<LoadedItemRequestDto> LoadedItems { get; set; } = new List<LoadedItemRequestDto>();
    }
}
