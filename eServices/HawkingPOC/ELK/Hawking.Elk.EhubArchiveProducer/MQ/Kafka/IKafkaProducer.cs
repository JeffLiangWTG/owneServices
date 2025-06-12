using System.Threading.Tasks;
using Hawking.Elk.EhubArchiveMessages.Model;

namespace Hawking.Elk.EhubArchiveProducer.MQ.Kafka
{
    interface IKafkaProducer
    {
        Task PublishMessageEvent(MessageEvent messageEvent);
    }
}
