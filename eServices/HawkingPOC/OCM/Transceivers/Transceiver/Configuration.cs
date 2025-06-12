using OcmPoc.Utils.Config;

namespace OcmPoc.Transceivers
{
	public class Configuration
	{
		public Configuration()
		{
			Provider = new ProviderConfig(RabbitMQ);
		}

		public KafkaConfig Kafka { get; } = new KafkaConfig();
		public MongoDbConfig MongoDB { get; } = new MongoDbConfig();
		public RabbitMqConfig RabbitMQ { get; } = new RabbitMqConfig();
		public SqlServerConfig SqlServer { get; } = new SqlServerConfig();

		public ProviderConfig Provider { get; }
	}
}
