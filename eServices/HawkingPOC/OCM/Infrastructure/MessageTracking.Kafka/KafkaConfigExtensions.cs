using System.Collections.Generic;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka
{
	static class KafkaConfigExtensions
	{
		public static Dictionary<string, object> ToDictionary(this KafkaConfig config)
		{
			return new Dictionary<string, object>
			{
				["bootstrap.servers"] = config.Brokers,
				["metadata.broker.list"] = config.Brokers,
				["group.id"] = config.Group
			};
		}
	}
}
