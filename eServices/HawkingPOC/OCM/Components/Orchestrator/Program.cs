using System;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageQueue.Rabbit;
using OcmPoc.Utils;
using OcmPoc.Utils.Config;

namespace OcmPoc.Components.Orchestrator
{
	class Program
    {
        static void Main(string[] args)
        {
			var configuration = BindConfiguration();
			var serviceProvider = ConfigureServices(configuration);

			Console.WriteLine(configuration);

			var baseQueueName = new QueueName(configuration.RabbitMq.NamePrefix);

			var queueSpecs = new QueueSpecCollection();
			var exchangeSpecs = new ExchangeSpecCollection();

			foreach (var queue in configuration.Queues.Select(q => QueueSpec.Parse(q)))
			{
				queueSpecs.Add(queue);
			}

			var routingExchange = ExchangeSpec.Parse(configuration.Exchange.Recipient);
			exchangeSpecs.Add(routingExchange, queueSpecs);

			foreach (var provider in configuration.Providers.Select(p => new ProviderConfig(configuration.RabbitMq, p)))
			{
				var bindingSpec = new HeaderBindingSpec(routingExchange.Name).Add("Recipient", provider.Name);
				queueSpecs.Add(QueueSpec.Parse(provider.ReceiveQueue))
						  .Add(QueueSpec.Parse(provider.SendQueue).WithBinding(bindingSpec));
			}

			using (serviceProvider.GetRequiredService<IConsoleCancelHandler>())
			{
				var queueClient = serviceProvider.GetRequiredService<IQueueClient>();
				queueClient.CreateExchanges(exchangeSpecs);
				queueClient.CreateDurableQueues(queueSpecs);
				queueClient.BindQueues(queueSpecs);
			}
		}

		static Configuration BindConfiguration()
		{
			var configuration = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.Build();
			
			var appConfig = new Configuration(configuration);
			configuration.Bind(appConfig);

			return appConfig;
		}

		internal static ServiceProvider ConfigureServices(Configuration config)
		{
			var serviceProvider =
				new ServiceCollection()
					.AddTransient<IConsoleCancelHandler, ConsoleCancelHandler>()
					.AddMessageQueueServices(config.RabbitMq)
					.BuildServiceProvider();

			return serviceProvider;
		}
	}
}
