using OcmPoc.Utils.Config;

namespace OcmPoc.Components.EventProcessor
{
	public class Configuration
	{
		public KafkaConfig Kafka { get; } = new KafkaConfig();
		public SqlServerConfig SqlServer { get; } = new SqlServerConfig();
	}
}
