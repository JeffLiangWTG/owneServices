namespace OcmPoc.Utils.Config
{
	public class KafkaConfig
	{
		public string Brokers { get; set; }
		public string Group { get; set; }

		public TopicConfig Topic { get; } = new TopicConfig();
	}
}
