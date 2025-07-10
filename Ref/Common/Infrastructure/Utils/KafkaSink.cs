using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using Confluent.Kafka;
using Serilog.Events;
using Serilog.Formatting;
using Serilog.Formatting.Elasticsearch;
using Serilog.Sinks.PeriodicBatching;

namespace CargoWise.RefDbRepo.Common.Utils
{
	public class KafkaSink : PeriodicBatchingSink
	{
		public KafkaSink(int batchSizeLimit, int period, string brokers, string topic, SecurityProtocol protocol, IProducer<Null, string> producer)
			: base(batchSizeLimit, TimeSpan.FromSeconds(period))
		{
			this.topic = topic;
			this.producer = producer;
		}

		public KafkaSink(int batchSizeLimit, int period, string brokers, string topic, SecurityProtocol protocol, int? messageTimeoutMs)
		 : base(batchSizeLimit, TimeSpan.FromSeconds(period))
		{
			this.topic = topic;
			var config = new ProducerConfig { BootstrapServers = brokers };
			config.MessageTimeoutMs = messageTimeoutMs;
			if (protocol == SecurityProtocol.Ssl)
			{
				config.SecurityProtocol = SecurityProtocol.Ssl;
				config.SslCaLocation = Path.Combine(GetBinFolder(), "WTGZone-Root-CA-1.cer");
			}
			producer = new ProducerBuilder<Null, string>(config).Build();
		}

		readonly string topic;
		readonly IProducer<Null, string> producer;
		readonly ITextFormatter formatter = new ElasticsearchJsonFormatter();

		protected override async Task EmitBatchAsync(IEnumerable<LogEvent> events)
		{
			foreach (var evt in events)
			{
				foreach (var property in evt.Properties)
				{
					if (property.Key != "Message" && property.Key != "SourceContext")
					{
						evt.RemovePropertyIfPresent(property.Key);
					}
				}
				if (evt.Properties.TryGetValue("Message", out var msgValue))
				{
					var structuredMsgValue = msgValue as StructureValue;
					if (structuredMsgValue != null)
					{
						evt.RemovePropertyIfPresent("Message");
						evt.AddPropertyIfAbsent(new LogEventProperty(structuredMsgValue.TypeTag, structuredMsgValue));
					}
				}
				using (var output = new StringWriter(CultureInfo.InvariantCulture))
				{
					formatter.Format(evt, output);
					await producer.ProduceAsync(topic, new Message<Null, string>() { Key = null, Value = output.ToString() });
				}
			}
		}

		static string GetBinFolder()
		{
			var uri = new UriBuilder(MigrationHelper.GetExecutingAssemblyLocation());
			string path = Uri.UnescapeDataString(uri.Path);
			var result = Path.GetDirectoryName(path);
			return result;
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			producer?.Dispose();
		}
	}
}
