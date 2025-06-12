using System.Threading;
using System.Threading.Tasks;
using Hawking.Elk.ElasticSearchLoggingConsumer.Config;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.MQ.Kafka
{
    interface IKafkaConsumer
    {
        Task RunAsync(IKafkaSubscription subscription, CancellationToken cancellationToken);
    }
}
