using System;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Utils;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka
{
	public class EventLogger : IEventLogger, IDisposable
	{
		readonly string eventTopic;
		readonly string logTopic;
		readonly Producer<int, MessageEvent> producer;

		public EventLogger(KafkaConfig config)
		{
			producer = new Producer<int, MessageEvent>(config.ToDictionary(), new IntSerializer(), new JsonSerialiser<MessageEvent>());
			eventTopic = config.Topic.Event;
			logTopic = config.Topic.Log;

			//producer.OnError += (s, e) => Console.WriteLine($"Kafka Error: {e.Reason}");
			producer.OnLog += (s, e) => Console.WriteLine($"Kafka Log: {e.Message}");
			producer.OnStatistics += (s, e) => Console.WriteLine($"Kafka Stats: {e}");
		}

		public async Task<long> LogEventAsync(MessageEvent messageEvent)
		{
			var (eventOffset, _) = 
				await Task.WhenAll(
					ProduceAsync(eventTopic, messageEvent),
					LogAsync(messageEvent)
				).ConfigureAwait(false);

			return eventOffset;
		}

		public async Task<long> LogAsync(MessageEvent messageEvent)
		{
			if (logTopic == null) { return -1; }

			return await ProduceAsync(logTopic, messageEvent).ConfigureAwait(false);
		}

		private async Task<long> ProduceAsync(string topic, MessageEvent messageEvent)
		{
			var messageFlowId = messageEvent.MessageFlowId;

			var sent = await producer.ProduceAsync(topic, messageFlowId, messageEvent).ConfigureAwait(false);

			if (sent.Error.HasError)
			{
				var (code, source, reason) = sent.Error;
				throw new Exception($"Error {code}: {reason} (Source: {source}, Topic: {topic})");
			}

			return sent.Offset.Value;
		}

		#region IDisposable Support
		private bool disposedValue = false; 

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					producer?.Dispose();
				}

				disposedValue = true;
			}
		}

		public void Dispose()
		{
			Dispose(true);
			GC.SuppressFinalize(this);
		}
		#endregion
	}
}
