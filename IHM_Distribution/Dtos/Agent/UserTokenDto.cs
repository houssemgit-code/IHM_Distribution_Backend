namespace IHM_Distribution.Dtos.Agent
{
	public class UserTokenDto
	{
		public string Token { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public string UserEmail { get; set; } = string.Empty;
		public DateTime Expiration { get; set; }
        public Guid Id { get; set; } // <-- Add this
    }
}
