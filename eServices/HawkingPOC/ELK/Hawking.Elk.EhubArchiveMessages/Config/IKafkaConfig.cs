using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Hawking.Elk.EhubArchiveMessages.Config
{
    public interface IKafkaConfig : IConfiguration
    {
        string KafkaTopic { get; set; }
        string KafkaMessageTypeName { get; set; }
        string KafkaBootstrapServers { get; set; }
        string GroupId { get; set; }
        bool EnableAutoCommit { get; set; }
        string AutoOffsetReset { get; set; }

        string ArchiveDbConnectionString { get; set; }
        string EhubClientDbConnectionString { get; set; }

        string ElasticSearchServerUrl { get; set; }
        string ElasticSearchIndex { get; set; }

        string ElasticSearchUserName { get; set; }
        string ElasticSearchPassword { get; set; }

        IEnumerable<KeyValuePair<string, string>> Producers { get; }
        IEnumerable<KeyValuePair<string, string>> Consumers { get; }
    }
}
