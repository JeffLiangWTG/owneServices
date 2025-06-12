namespace OcmPoc.Utils.Config
{
	public class RabbitMqConfig
	{
		public string Host { get; set; }
		public string Username { get; set; }
		public string Password { get; set; }
		public string NamePrefix { get; set; }
		public ushort PreFetch { get; set; } = 1;

		public QueueConfig Queue { get; } = new QueueConfig();
		public BindingConfig Input { get; } = new BindingConfig();
		public BindingConfig Output { get; } = new BindingConfig();
	}
}
