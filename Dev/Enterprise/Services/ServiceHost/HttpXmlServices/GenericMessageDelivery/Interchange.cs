namespace Enterprise.Services.ServiceHost.GMD
{
	public sealed class Interchange
	{
		public string SenderId { get; set; }
		public string RecipientId { get; set; }
		public string InterchangeType { get; set; }
		public string Body { get; set; }
	}
}
