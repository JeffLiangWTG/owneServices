using OcmPoc.Utils.Config;

namespace OcmPoc.Components.Paralleliser
{
	public class Configuration
    {
		public RabbitMqConfig RabbitMq { get; } = new RabbitMqConfig();
		public string PartitionHeaders { get; set; }
	}
}
