using System;
using System.Collections.Generic;
using System.Linq;
using Hawking.Elk.EhubArchiveMessages.Config;
using Hawking.Elk.ElasticSearchLoggingConsumer.MQ.Kafka;
using Unity;
using Unity.Resolution;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.Config
{
    public class ConsumerConfig : IConsumerConfig
    {
        const string PathConsumer = ":consumer";
        const string PathSubscriptions = ":subscriptions";

        public ConsumerConfig(IUnityContainer container, IKafkaConfig kafkaConfig)
        {
            Subscriptions = new List<IKafkaSubscription>();

            var consumer = kafkaConfig.Consumers.FirstOrDefault(x => x.Value == typeof(KafkaConsumer).FullName);
            if (!string.IsNullOrEmpty(consumer.Key))
            {
                var consumerPath = consumer.Key.Substring(0,
                    consumer.Key.LastIndexOf(PathConsumer, StringComparison.InvariantCultureIgnoreCase));

                for (var index = 0;; index++)
                {
                    var keyPattern = $"{consumerPath}{PathSubscriptions}:{index}:";
                    var keyIndex = keyPattern.Length;
                    var settings = kafkaConfig.Consumers
                        .Where(x => x.Key.StartsWith(keyPattern, StringComparison.InvariantCultureIgnoreCase))
                        .Select(x => new {x.Key, x.Value})
                        .ToDictionary(x => x.Key.Substring(keyIndex), x => x.Value);

                    if (!settings.Any())
                    {
                        break;
                    }

                    Subscriptions.Add(container.Resolve<IKafkaSubscription>(new ParameterOverride("settings", settings)));
                }
            }
        }

        public IList<IKafkaSubscription> Subscriptions { get; }
    }
}
