using Confluent.Kafka;
using Serilog;
using Serilog.Configuration;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public static class KafkaSinkLoggerConfigurationExtensions
	{
		public static LoggerConfiguration Kafka(this LoggerSinkConfiguration sinkConfiguration, int batchSizeLimit, int period, string brokers, string topic, SecurityProtocol protocol = SecurityProtocol.Plaintext, int? messageTimeoutMs = null)
		{
			Argument.Argument.NotNull(sinkConfiguration, nameof(sinkConfiguration));
			var sink = new KafkaSink(batchSizeLimit, period, brokers, topic, protocol, messageTimeoutMs);
			return sinkConfiguration.Sink(sink);
		}
	}
}
