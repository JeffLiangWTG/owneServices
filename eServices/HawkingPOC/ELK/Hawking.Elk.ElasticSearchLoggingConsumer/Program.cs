using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Hawking.Elk.Common.Config;
using Hawking.Elk.Common.Unity;
using Hawking.Elk.EhubArchiveMessages.Config;
using Hawking.Elk.ElasticSearchLoggingConsumer.Config;
using Hawking.Elk.ElasticSearchLoggingConsumer.MQ.ElasticSearch;
using Hawking.Elk.ElasticSearchLoggingConsumer.MQ.Kafka;
using log4net;
using Microsoft.Extensions.Configuration;
using Unity;
using Unity.Lifetime;
using Unity.log4net;

namespace Hawking.Elk.ElasticSearchLoggingConsumer
{
    class Program
    {
        static IUnityContainer Container { get; set; }

        static void Main(string[] args)
        {
            Console.WriteLine("Hello World from Hawking.Elk.ElasticSearchLoggingConsumer!");
            Container = DependencyFactory.Container;
            ConfigUnityContainer();

            var exitEvent = new ManualResetEvent(false);
            var cancelSource = new CancellationTokenSource();

            Console.CancelKeyPress += (sender, eventArgs) =>
            {
                eventArgs.Cancel = true;
                cancelSource.Cancel();
                exitEvent.Set();
            };

            var elasticClient = Container.Resolve<IElasticSearchClient>();
            var pingResponse = elasticClient.ElasticClient.Ping();
            if (!pingResponse.IsValid)
            {
                Console.WriteLine(pingResponse.DebugInformation);
                exitEvent.WaitOne();
            }
            else
            {
                var tasks = new List<Task>();
                var consumerConfig = Container.Resolve<IConsumerConfig>();
                foreach (var subscription in consumerConfig.Subscriptions)
                {
                    var kafkaConsumer = Container.Resolve<IKafkaConsumer>();
                    var task =
                        Task.Run(() =>
                            kafkaConsumer.RunAsync(subscription, cancelSource.Token).ContinueWith(
                                x =>
                                {
                                    if (x?.Exception != null)
                                    {
                                        Console.WriteLine(x.Exception.Message);
                                    }

                                    tasks.Remove(x);
                                }));

                    tasks.Add(task);
                }

                exitEvent.WaitOne();
                Task.WaitAll(tasks.ToArray());
            }
        }

        static void ConfigUnityContainer()
        {
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
            Container.RegisterType<IKafkaSubscription, KafkaSubscription>();
            Container.RegisterType<IConsumerConfig, ConsumerConfig>(new ContainerControlledLifetimeManager());

            Container.RegisterType<IIniFile, IniFile>();

            Container.RegisterType<IKafkaConsumer, KafkaConsumer>();
            Container.RegisterType<IElasticSearchClient, ElasticSearchClient>(new ContainerControlledLifetimeManager());
        }

        static void ConfigLogger()
        {
            Container.AddNewExtension<Log4NetExtension>();
            Container.RegisterInstance(LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType));
        }
    }
}
