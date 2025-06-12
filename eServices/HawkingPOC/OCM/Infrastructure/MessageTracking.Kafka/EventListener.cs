using System;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Newtonsoft.Json;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;
using OcmPoc.Infrastructure.MessageInterfaces.Tracking;
using OcmPoc.Utils.Config;

namespace OcmPoc.Infrastructure.MessageTracking.Kafka
{
	public class EventListener : IDisposable, IEventListener
	{
		readonly CancellationTokenSource cts;
		readonly Consumer<int, MessageEvent> consumer;
		readonly Task listener;

		public event EventHandler<string> OnLog;

		public EventListener(KafkaConfig config)
		{
			consumer = new Consumer<int, MessageEvent>(config.ToDictionary(), new IntDeserializer(), new JsonSerialiser<MessageEvent>());
			Events = Observable.FromEventPattern<Message<int, MessageEvent>>(h => consumer.OnMessage += h, h => consumer.OnMessage -= h)
							   .Select(m => m.EventArgs.Value);
			consumer.OnMessage += (s, e) => Log($"Received {e.Offset}: Message {JsonConvert.SerializeObject(e.Value)}");
			consumer.OnError += (s, e) => Log($"Error: {e.Code} : {e.Reason}");

			consumer.Subscribe(config.Topic.Event);

			cts = new CancellationTokenSource();
			listener = Task.Run(() => PollForEvents(cts.Token));
			Log("Listening");
		}

		public IObservable<MessageEvent> Events { get; }

		void PollForEvents(CancellationToken token)
		{
			while(!token.IsCancellationRequested)
			{
				consumer.Poll(TimeSpan.FromSeconds(0.5));
			}
		}

		void Log(string message)
		{
			OnLog?.Invoke(this, message);
		}

		#region IDisposable Support
		private bool disposedValue = false; // To detect redundant calls

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					cts.Cancel();
					listener.Wait();
					consumer.Unsubscribe();
					consumer?.Dispose();
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
