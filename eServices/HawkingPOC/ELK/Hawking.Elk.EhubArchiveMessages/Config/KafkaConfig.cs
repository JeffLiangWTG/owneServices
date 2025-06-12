using System.Collections.Generic;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Hawking.Elk.EhubArchiveMessages.Config
{
    public class KafkaConfig : IKafkaConfig
    {
        const string KeyKafkaTopic = "KafkaTopic";
        const string KeyKafkaMessageTypeName = "KafkaDocumentTypeName";
        const string KeyKafkaBootstrapServers = "Kafka.bootstrap.servers";
        const string KeyGroupId = "group.id";
        const string KeyEnableAutoCommit = "enable.auto.commit";
        const string KeyAutoOffsetReset = "auto.offset.reset";
        const string KeyArchiveDbConnectionString = "ArchiveDbContext";
        const string KeyEhubClientDbConnectionString = "EhubClientDbContext";
        const string KeyElasticSearchServerUrl = "ElasticSearch:ServerUrl";
        const string KeyElasticSearchIndex = "ElasticSearch:DefaultIndex";
        const string KeyElasticSearchUserName = "ElasticSearchUserName";
        const string KeyElasticSearchPassword = "ElasticSearchPassword";

        readonly IConfigurationRoot configuration;

        public KafkaConfig(IConfigurationRoot configurationRoot)
        {
            configuration = configurationRoot;

            KafkaTopic = configuration[KeyKafkaTopic] ?? "Hawking.Elk.EhubArchiveProducer.Model.EhubArchiveMessageLog";
            KafkaBootstrapServers = configuration[KeyKafkaBootstrapServers] ?? "localhost:9092";
            GroupId = configuration[KeyGroupId] ?? "hawking.tracking.consumer-group";
            if (configuration[KeyEnableAutoCommit] != null && bool.TryParse(configuration[KeyEnableAutoCommit], out var result))
            {
                EnableAutoCommit = result;
            }
            AutoOffsetReset = configuration[KeyAutoOffsetReset] ?? "earliest";

            ArchiveDbConnectionString = configuration.GetConnectionString(KeyArchiveDbConnectionString);
            EhubClientDbConnectionString = configuration.GetConnectionString(KeyEhubClientDbConnectionString);

            ElasticSearchServerUrl = configuration[KeyElasticSearchServerUrl] ?? "http://localhost:9200";
            ElasticSearchIndex = configuration[KeyElasticSearchIndex] ?? "hawking";

            ElasticSearchUserName = configuration[KeyElasticSearchUserName] ?? string.Empty;
            ElasticSearchPassword = configuration[KeyElasticSearchPassword] ?? string.Empty;
            KafkaMessageTypeName = configuration[KeyKafkaMessageTypeName] ?? "doc";

            Producers = configuration.GetSection("Kafka").GetSection("Producers").AsEnumerable();
            Consumers = configuration.GetSection("Kafka").GetSection("Consumers").AsEnumerable();
        }

        public string this[string key]
        {
            get => configuration[key];
            set => configuration[key] = value;
        }

        public string KafkaTopic { get; set;  }
        public string KafkaMessageTypeName { get; set; }
        public string KafkaBootstrapServers { get; set; }
        public string GroupId { get; set; }
        public bool EnableAutoCommit { get; set; }
        public string AutoOffsetReset { get; set; }
        public string ArchiveDbConnectionString { get; set; }
        public string EhubClientDbConnectionString { get; set; }
        public string ElasticSearchServerUrl { get; set; }
        public string ElasticSearchIndex { get; set; }
        public string ElasticSearchUserName { get; set; }
        public string ElasticSearchPassword { get; set; }
        public IEnumerable<KeyValuePair<string, string>> Producers { get; }
        public IEnumerable<KeyValuePair<string, string>> Consumers { get; }

        public IEnumerable<IConfigurationSection> GetChildren()
        {
            return configuration.GetChildren();
        }

        public IChangeToken GetReloadToken()
        {
            return configuration.GetReloadToken();
        }

        public IConfigurationSection GetSection(string key)
        {
            return configuration.GetSection(key);
        }
    }
}
