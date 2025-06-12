using System.Threading.Tasks;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;
using OcmPoc.Infrastructure.MessageQueue.Rabbit;
using OcmPoc.Utils;

namespace OcmPoc.Components.Paralleliser
{
	class Program
	{
		static async Task Main(string[] args)
		{
			var configuration = BindConfiguration();
			var serviceProvider = ConfigureServices(configuration);

			var specs = new QueueSpecCollection()
				.Add(new QueueSpec(configuration.RabbitMq.Queue.InternalInput)
					.WithDeadLetterQueue(new QueueSpec(configuration.RabbitMq.Queue.InternalDeadLetter)))
				.Add(new QueueSpec(configuration.RabbitMq.Queue.InternalOutput));
			
			using (var cancelHandler = new ConsoleCancelHandler())
			{
				await serviceProvider.GetRequiredService<Paralleliser>()
									 .RunAsync(configuration, specs, cancelHandler.Token);
			}
		}

		static Configuration BindConfiguration()
		{
			var configuration = new ConfigurationBuilder()
				.AddEnvironmentVariables()
				.Build();

			var appConfig = new Configuration();
			configuration.Bind(appConfig);

			return appConfig;
		}

		internal static ServiceProvider ConfigureServices(Configuration config)
		{
			var mapperConfig = new MapperConfiguration(cfg =>
			{
				cfg.CreateMap<QueueItem, IndexedQueueItem>();
				cfg.CreateMap<IndexedQueueItem, QueueItem>();
			});

			var serviceProvider = 
				new ServiceCollection()
					.AddSingleton<Paralleliser>()
					.AddSingleton(mapperConfig.CreateMapper())
					.AddTransient<IConsoleCancelHandler, ConsoleCancelHandler>()
					.AddMessageQueueServices(config.RabbitMq)
					.BuildServiceProvider();

			return serviceProvider;
		}
	}
}
