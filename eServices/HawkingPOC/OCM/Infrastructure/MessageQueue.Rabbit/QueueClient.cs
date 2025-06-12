using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Utils.Config;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	internal class QueueClient : IQueueClient, IDisposable
	{
		readonly IConnection connection;
		readonly QueueConfig queues;
		readonly ushort preFetch;

		public QueueClient(RabbitMqConfig config, IConnectionFactory connectionFactory)
			: this(config, connectionFactory.CreateConnection())
		{ 
		}

		public QueueClient(RabbitMqConfig config, IConnection connection)
		{
			this.connection = connection;
			queues = config.Queue;
			preFetch = config.PreFetch;
		}

		public IDisposable CreateTemporaryQueues(IQueueSpecCollection specs)
		{
			ForEachSpec(specs, (channel, spec) =>
			{
				channel.QueueDeclare(spec.Name, false, false, true, spec.Configuration);
				channel.QueuePurge(spec.Name);
			});

			return Disposable.Create(() =>
			{
				ForEachSpec(specs, (channel, spec) =>
				{
					channel.QueueDelete(spec.Name, false, false);
				});
			});
		}

		public void CreateDurableQueues(IQueueSpecCollection specs)
		{
			ForEachSpec(specs, (channel, spec) =>
			{
				var xch = spec.Configuration.TryGetValue(Headers.XDeadLetterExchange, out object value) ? value.ToString() : "";
				var key = spec.Configuration.TryGetValue(Headers.XDeadLetterRoutingKey, out value) ? value.ToString() : "";

				Console.WriteLine($"Declaring queue '{spec.Name}' with deadletter routing '{xch}:{key}'");
				channel.QueueDeclare(spec.Name, true, false, false, spec.Configuration);
			});
		}

		void ForEachSpec<TSpec>(IEnumerable<TSpec> specs, Action<IModel, TSpec> action)
		{
			using (var channel = connection.CreateModel())
			{
				foreach (var spec in specs)
				{
					action.Invoke(channel, spec);
				}
			}
		}

		public void CreateExchange(IExchangeSpec spec)
		{
			using (var channel = connection.CreateModel())
			{
				channel.ExchangeDeclare(spec.Name, spec.Type, true, false, null);
			}
		}

		public void CreateExchanges(IExchangeSpecCollection specs)
		{
			ForEachSpec(specs, (channel, spec) =>
			{
				channel.ExchangeDeclare(spec.Name, spec.Type, true, false, spec.Configuration);
			});
		}


		public void BindQueues(IQueueSpecCollection specs)
		{
			ForEachSpec(specs, (channel, spec) =>
			{
				foreach (var binding in spec.Bindings)
				{
					channel.QueueBind(spec.Name, binding.ExchangeName, binding.RoutingKey, binding.BindArguments);
				}
			});
		}

		public IExchangeWriter GetExchangeWriter(string exchangeName)
		{
			return new ExchangeWriter(connection, exchangeName);
		}

		public IQueueListener<TQueueItem> GetQueueListener<TQueueItem>(string queueName = null)
			where TQueueItem : QueueItem, new()
		{
			return new QueueListener<TQueueItem>(connection, queueName ?? queues.Input, preFetch);
		}

		public IQueueReader<TQueueItem> GetQueueReader<TQueueItem>(string queueName = null) 
			where TQueueItem : QueueItem, new()
		{
			return new QueueReader<TQueueItem>(connection, queueName ?? queues.Input);
		}

		public IQueueWriter GetQueueWriter(string queueName = null)
		{
			queueName = queueName ?? queues.Output;
			return new QueueWriter(connection, queueName ?? queues.Output);
		}

		#region IDisposable Support

		private bool disposedValue = false;

		protected virtual void Dispose(bool disposing)
		{
			if (!disposedValue)
			{
				if (disposing)
				{
					connection?.Dispose();
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
