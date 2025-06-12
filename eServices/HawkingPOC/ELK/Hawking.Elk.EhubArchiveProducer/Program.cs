using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hawking.Elk.Common.Unity;
using Hawking.Elk.EhubArchiveMessages.Config;
using Hawking.Elk.EhubArchiveMessages.Service;
using Hawking.Elk.EhubArchiveProducer.Config;
using Hawking.Elk.EhubArchiveProducer.Helper;
using Hawking.Elk.EhubArchiveProducer.MQ.Kafka;
using log4net;
using Microsoft.Extensions.Configuration;
using Unity;
using Unity.Lifetime;
using Unity.log4net;

namespace Hawking.Elk.EhubArchiveProducer
{
    class Program
    {
        static IUnityContainer Container { get; set; }

        static void Main(string[] args)
        {
            Container = DependencyFactory.Container;

            var exitEvent = new ManualResetEvent(false);
            var cancelSource = new CancellationTokenSource();

            Console.WriteLine("Hello World from Hawking.Elk.EhubArchiveProducer!");
            ConfigUnityContainer();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancelSource.Cancel();
                exitEvent.Set();
            };

            var task = Task.Run(() => LoadEhubArchiveMessages(cancelSource.Token))
                .ContinueWith((e) =>
                {
                    if (e.Exception != null)
                    {
                        Console.WriteLine($"Error: {e.Exception.StackTrace}");
                    }

                    exitEvent.Set();
                });

            exitEvent.WaitOne();
            task.Wait();
        }

        static void LoadEhubArchiveMessages(CancellationToken cancelToken)
        {
            Console.WriteLine($"{nameof(LoadEhubArchiveMessages)}:enter");

            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                {
                    Console.WriteLine("User has cancelled");
                    return;
                }

                var lastReadArchivedUtc = EhubArchiveMessageLoadingConfig.LastReadArchivedUtc;
                var loadingService = Container.Resolve<IEhubArchiveMessagesLoadingService>();
                var messages =
                    loadingService.LoadEhubArchiveMessages(
                        lastReadArchivedUtc,
                        EhubArchiveMessageLoadingConfig.LoadBatchSize);

                if (messages == null || !messages.Any())
                {
                    Console.WriteLine("No more messages to load, enter any key to terminate.");
                    Console.ReadKey();

                    break;
                }

                Console.WriteLine($"Loaded: {messages.Count()} messages");

                foreach (var msg in messages)
                {
                    var messageEvent = EhubArchiveMessageLogHelper.CreateEhubArchiveMessageEvent(msg, EhubArchiveMessagesLoadingService.EhubClients);

                    var producer = Container.Resolve<IKafkaProducer>();
                    producer.PublishMessageEvent(messageEvent);

                    Console.WriteLine($"ArchivedUTC:{messageEvent.Message.ArchivedUTC}, {messageEvent.Message.SenderInbox} >> {messageEvent.Message.RecipientInbox}, Status:{messageEvent.Message.Status}");

                    if (msg.ArchivedUTC > lastReadArchivedUtc)
                    {
                        lastReadArchivedUtc = msg.ArchivedUTC;
                    }
                }

                EhubArchiveMessageLoadingConfig.UpdateLastReadArchivedUtc(lastReadArchivedUtc);
            }

            Console.WriteLine($"{nameof(LoadEhubArchiveMessages)}:leave");
        }

        static void ConfigUnityContainer()
        {
            Console.WriteLine($"{nameof(ConfigUnityContainer)}:enter");

            ConfigLogger();

            var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

            var configurationBuilder = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddJsonFile($"appsettings.{environment}.json", true)
                .AddEnvironmentVariables();

            var config = configurationBuilder.Build();
            var kafkaConfig = new KafkaConfig(config);
            Container.RegisterInstance<IConfigurationRoot>(config);
            Container.RegisterInstance<IKafkaConfig>(kafkaConfig);

            Container.RegisterType<IEhubArchiveMessagesLoadingService, EhubArchiveMessagesLoadingService>();
            Container.RegisterType<IKafkaProducer, KafkaProducer>(new ContainerControlledLifetimeManager());

            Console.WriteLine($"{nameof(ConfigUnityContainer)}:leave");
        }

        static void ConfigLogger()
        {
            Container.AddNewExtension<Log4NetExtension>();
            Container.RegisterInstance<ILog>(LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
        }
    }
}
