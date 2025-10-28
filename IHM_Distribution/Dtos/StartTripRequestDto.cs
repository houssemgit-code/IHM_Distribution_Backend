namespace IHM_Distribution.Dtos
{
    public class StartTripRequestDto
    {
        public Guid AgentId { get; set; }
        public List<LoadedItemRequestDto> LoadedItems { get; set; } = new List<LoadedItemRequestDto>();
    }
}
