using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Confluent.Kafka;
using Confluent.Kafka.Serialization;
using Hawking.Elk.EhubArchiveMessages.Config;
using Hawking.Elk.EhubArchiveMessages.Model;
using log4net;
using Newtonsoft.Json;

namespace Hawking.Elk.EhubArchiveProducer.MQ.Kafka
{
    public class KafkaProducer : IKafkaProducer
    {
        const string PathTopic = ":topic";
        const string PathProducer = ":producer";
        const string PathKafkaServer = ":bootstrap.servers";

        readonly ILog logger;
        readonly Dictionary<string, object> KafkaProducerConfig;
        readonly string KafkaTopic;

        public KafkaProducer(ILog logger, IKafkaConfig kafkaConfig)
        {
            this.logger = logger;

            var producerIndex = kafkaConfig.Producers.FirstOrDefault(x => x.Value == GetType().FullName);
            if (!string.IsNullOrEmpty(producerIndex.Key))
            {
                var producerPath = producerIndex.Key.Substring(0,
                    producerIndex.Key.IndexOf(PathProducer, StringComparison.Ordinal));

                KafkaTopic = kafkaConfig.Producers.FirstOrDefault(x => x.Key == producerPath + PathTopic).Value;

                KafkaProducerConfig = new Dictionary<string, object>
                {
                    {
                        PathKafkaServer.Substring(1),
                        kafkaConfig.Producers.FirstOrDefault(x => x.Key == $"{producerPath}{PathKafkaServer}").Value
                    }
                };
            }
        }

        public async Task PublishMessageEvent(MessageEvent messageEvent)
        {
            using (var producer = new Producer<Null, string>(KafkaProducerConfig, null, new StringSerializer(Encoding.UTF8)))
            {
                logger.Info($"Publishing message with tracking ID '{messageEvent.Message.TrackingId}'");

                var message = JsonConvert.SerializeObject(messageEvent);
                var result = await producer.ProduceAsync(KafkaTopic, null, message);

                logger.Debug($"Published message to {result.TopicPartitionOffset}");
            }
        }
    }
}
