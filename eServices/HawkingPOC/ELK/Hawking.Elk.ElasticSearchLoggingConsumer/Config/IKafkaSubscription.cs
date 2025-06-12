using System.Collections.Generic;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.Config
{
    public interface IKafkaSubscription
    {
        string KafkaBootstrapServer { get; }
        string Topic { get; }
        int Partition { get; set; }
        long Offset { get; set; }

        string MessageType { get; }
        string ConsumerGroupId { get; }
        string ConfigFilePath { get; }
        IDictionary<string, object> KafkaConsumerConfig { get; }

        dynamic GetValue(string key);
        void SetValue(string key, dynamic value);
        void UpdateAndSaveKeyValues();
    }
}
