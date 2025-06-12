using System;
using System.Collections.Generic;
using System.IO;
using Hawking.Elk.Common.Config;
using Unity;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.Config
{
    public class KafkaSubscription : IKafkaSubscription
    {
        const string KeyTopic = "topic";
        const string KeyMessageType = "message.type";
        const string KeyConfigFilePath = "config.filepath";
        const string IniFileExtension = ".ini";

        const string KeyGroupId = "group.id";
        const string KeyBootstrapServers = "bootstrap.servers";
        const string KeyAutoCommit = "auto.offset.reset";
        const string KeyAutoOffsetReset = "auto.offset.reset";

        readonly List<string> ConsumerKeys = new List<string>
        {
            KeyGroupId,
            KeyBootstrapServers,
            KeyAutoCommit,
            KeyAutoOffsetReset
        };

        public KafkaSubscription(IUnityContainer container, IDictionary<string, string> settings)
        {
            KafkaConsumerConfig = new Dictionary<string, object>();
            foreach (var setting in settings)
            {
                if (ConsumerKeys.Contains(setting.Key))
                {
                    KafkaConsumerConfig.Add(setting.Key, setting.Value);
                }
            }

            KafkaBootstrapServer = settings.ContainsKey(KeyBootstrapServers) ? settings[KeyBootstrapServers] : "localhost:9092";
            Topic = settings.ContainsKey(KeyTopic) ? settings[KeyTopic] : string.Empty;
            MessageType = settings.ContainsKey(KeyMessageType) ? settings[KeyMessageType] : "doc";
            ConsumerGroupId = settings.ContainsKey(KeyGroupId) ? settings[KeyGroupId] : GetType().FullName;

            var configFile = settings.ContainsKey(KeyConfigFilePath) ? settings[KeyGroupId] : Topic + IniFileExtension;
            ConfigFilePath = !File.Exists(configFile) ? Path.Combine(AppDomain.CurrentDomain.BaseDirectory, configFile) : configFile;
            if (string.Compare(IniFileExtension, Path.GetExtension(ConfigFilePath), StringComparison.InvariantCultureIgnoreCase) != 0)
            {
                ConfigFilePath += IniFileExtension;
            }

            IniFile = container.Resolve<IIniFile>();
            IniFile.Load(ConfigFilePath);
        }

        public string KafkaBootstrapServer { get; }
        public string Topic { get; }
        public string MessageType { get; }
        public string ConsumerGroupId { get; }
        public string ConfigFilePath { get; }

        public int Partition
        {
            get
            {
                var partitionValue = GetTopicValue(nameof(Partition));
                if (partitionValue is int)
                {
                    return partitionValue;
                }

                return int.TryParse(partitionValue.ToString(), out int value) ? value : 0;
            }

            set => SetTopicValue(nameof(Partition), value);
        }

        public long Offset
        {
            get
            {
                var partitionValue = GetTopicValue(nameof(Partition));
                if (partitionValue is int)
                {
                    return partitionValue;
                }

                return int.TryParse(partitionValue.ToString(), out int value) ? value : 0L;
            }
            set => SetTopicValue(nameof(Partition), value);
        }

        dynamic GetTopicValue(string key)
        {
            return IniFile.GetValue(Topic + "_" + key);
        }

        void SetTopicValue(string key, dynamic value)
        {
            IniFile.SetValue(Topic + "_" + key, value);
        }

        IIniFile IniFile { get; }

        public IDictionary<string, object> KafkaConsumerConfig { get; }

        public dynamic GetValue(string key)
        {
            return IniFile.GetValue(key);
        }

        public void SetValue(string key, dynamic value)
        {
            IniFile.SetValue(key, value);
        }

        public void UpdateAndSaveKeyValues()
        {
            IniFile.UpdateAndSave();
        }
    }
}