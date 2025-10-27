namespace IHM_Distribution.Dtos
{
    public class StartTripRequestDto
    {
        public int AgentId { get; set; }
        public List<LoadedItemRequest> LoadedItems { get; set; } = new List<LoadedItemRequest>();
    }
}
