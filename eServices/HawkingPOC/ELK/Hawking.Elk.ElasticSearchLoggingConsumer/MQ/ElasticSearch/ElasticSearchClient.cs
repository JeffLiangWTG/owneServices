using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Elasticsearch.Net;
using Hawking.Elk.EhubArchiveMessages.Config;
using log4net;
using Nest;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.MQ.ElasticSearch
{
    public class ElasticSearchClient : IElasticSearchClient
    {
        readonly ElasticClient elasticClient;
        readonly ILog logger;

        public ElasticSearchClient(ILog logger, IKafkaConfig kafkaConfig)
        {
            this.logger = logger;

            var settings =
                new ConnectionSettings(new Uri(kafkaConfig.ElasticSearchServerUrl))
                    .DisableDirectStreaming()
                    .EnableDebugMode();

            if (!string.IsNullOrEmpty(kafkaConfig.ElasticSearchUserName))
            {
                settings.BasicAuthentication(kafkaConfig.ElasticSearchUserName, kafkaConfig.ElasticSearchPassword);
            }

            elasticClient = new ElasticClient(settings);
        }

        public IElasticClient ElasticClient => elasticClient;

        public ICreateIndexResponse CreateIndex<T>(string indexName) where T : class
        {
            return elasticClient.CreateIndex(indexName, idx => idx.Mappings(x => x.Map<T>(m => m.AutoMap())));
        }

        public async Task<IBulkResponse> IndexManyAsync<T>(
            T[] messages,
            string indexNames,
            Type typeName = null,
            CancellationToken cancellationToken = default(CancellationToken)) where T : class
        {
            return await elasticClient.IndexManyAsync(messages, cancellationToken: cancellationToken);
        }

        public IResponse Index(string jsonString, string indexName, string typeName)
        {
            var existRespone = elasticClient.IndexExists(indexName);
            if (!existRespone.IsValid)
            {
                logger.Error(existRespone.DebugInformation);
                return existRespone;
            }

            if (!existRespone.Exists)
            {
                var response = elasticClient.CreateIndex(indexName);
                if (!response.IsValid)
                {
                    logger.Error(existRespone.DebugInformation);
                    return response;
                }
            }

            return elasticClient.LowLevel.Index<IndexResponse>(indexName, typeName, jsonString);
        }
    }
}