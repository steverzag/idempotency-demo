namespace IdempotencyDemo.API.DTOs
{
	internal sealed record IdempotentRequestAttempt(string HashedRequestBody, string EndpointTarget)
	{
		public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
		public int AttempNumber { get; set; } = 1;
	}
}
