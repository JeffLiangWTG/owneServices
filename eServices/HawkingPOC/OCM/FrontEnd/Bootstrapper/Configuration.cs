using OcmPoc.Utils.Config;

namespace OcmPoc.FrontEnd.Bootstrapper
{
	public class Configuration
    {
		public bool ConfiguredForTest { get; set; }
		public RabbitMqConfig RabbitMq { get; } = new RabbitMqConfig();
		public MongoDbConfig MongoDb { get; } = new MongoDbConfig();
		public KafkaConfig Kafka { get; } = new KafkaConfig();
		public SqlServerConfig SqlServer { get; } = new SqlServerConfig();
	}
}
