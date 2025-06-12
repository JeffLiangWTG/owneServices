using OcmPoc.Utils.Config;

namespace OcmPoc.Mapping.Mapper
{
	public class Configuration
	{
		public RabbitMqConfig RabbitMq { get; } = new RabbitMqConfig();
		public MongoDbConfig MongoDb { get; } = new MongoDbConfig();
		public KafkaConfig Kafka { get; } = new KafkaConfig();
		public SqlServerConfig SqlServer { get; } = new SqlServerConfig();
		public MappingMode MappingMode { get; set; }
	}
}
