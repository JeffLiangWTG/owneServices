using System.Collections.Generic;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.Config
{
    public interface IConsumerConfig
    {
        IList<IKafkaSubscription> Subscriptions { get; }
    }
}
