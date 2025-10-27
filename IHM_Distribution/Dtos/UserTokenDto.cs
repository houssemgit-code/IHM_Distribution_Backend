namespace IHM_Distribution.Dtos
{
	public class UserTokenDto
	{
		public string Token { get; set; } = string.Empty;
		public string Name { get; set; } = string.Empty;
		public DateTime Expiration { get; set; }
	}
}
