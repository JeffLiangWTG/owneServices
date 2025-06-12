namespace OcmPoc.Utils.Config
{
	public class ProviderConfig
	{
		readonly RabbitMqConfig rabbitMq;

		public ProviderConfig(RabbitMqConfig rabbitMq)
		{
			this.rabbitMq = rabbitMq;
		}

		public ProviderConfig(RabbitMqConfig rabbitMq, string name)
			: this(rabbitMq)
		{
			Name = name;
		}

		public string Name { get; set; }

		QueueName BaseQueueName => new QueueName(rabbitMq.NamePrefix);

		public QueueName SendQueue => BaseQueueName.Combine(Name.ToLower(), "send");
		public QueueName ReceiveQueue => BaseQueueName.Combine("receive>");
	}
}
