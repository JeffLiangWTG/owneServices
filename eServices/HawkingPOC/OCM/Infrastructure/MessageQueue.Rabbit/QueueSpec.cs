using System;
using System.Collections.Generic;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Utils;
using RabbitMQ.Client;

namespace OcmPoc.Infrastructure.MessageQueue.Rabbit
{
	public class QueueSpec : IQueueSpec
	{
		List<IBindingSpec> bindings;

		public QueueSpec(string queueName)
		{
			Name = queueName;
			Configuration = new Dictionary<string, object>();
			bindings = new List<IBindingSpec>();
		}

		public string Name { get; }

		public IDictionary<string, object> Configuration { get; }

		public IQueueSpec DeadLetterQueue { get; private set; }

		public IEnumerable<IBindingSpec> Bindings => bindings;

		public IQueueSpec WithDeadLetterQueue(IQueueSpec spec)
		{
			Configuration[Headers.XDeadLetterExchange] = string.Empty;
			Configuration[Headers.XDeadLetterRoutingKey] = spec.Name;
			DeadLetterQueue = spec;

			return this;
		}

		public IQueueSpec WithBinding(IBindingSpec spec)
		{
			bindings.Add(spec);
			return this;
		}

		public static IQueueSpec Parse(string spec)
		{
			string[] queueNames = spec.Split('>');
			var primaryQueue = queueNames[0];
			
			if (string.IsNullOrWhiteSpace(primaryQueue))
			{
				throw new ArgumentException($"'{spec}' is an invalid queue specification", nameof(spec));
			}

			var queueSpec = new QueueSpec(primaryQueue);

			if (queueNames.Length > 1)
			{
				var deadLetterQueue = queueNames[1];

				if (string.IsNullOrWhiteSpace(deadLetterQueue))
				{
					deadLetterQueue = new QueueName(primaryQueue).Combine("dl");
				}

				var deadLetterSpec = new QueueSpec(deadLetterQueue);
				queueSpec.WithDeadLetterQueue(deadLetterSpec);
			}

			return queueSpec;
		}
	}
}
