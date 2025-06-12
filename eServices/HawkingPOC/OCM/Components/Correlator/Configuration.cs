using OcmPoc.Utils.Config;

namespace OcmPoc.Components.Correlator
{
	public class Configuration
	{
		public KafkaConfig Kafka { get; } = new KafkaConfig();
		public RabbitMqConfig RabbitMQ { get; } = new RabbitMqConfig();
		public SqlServerConfig SqlServer { get; } = new SqlServerConfig();
	}
}
