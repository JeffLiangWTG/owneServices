using System;
using System.Threading;
using System.Threading.Tasks;
using Nest;

namespace Hawking.Elk.ElasticSearchLoggingConsumer.MQ.ElasticSearch
{
    public interface IElasticSearchClient
    {
        ICreateIndexResponse CreateIndex<T>(string indexName) where T : class;
        IResponse Index(string jsonString, string indexName, string typeName);
        Task<IBulkResponse> IndexManyAsync<T>(T[] messages, string indexNames, Type typeName = null,
            CancellationToken cancellationToken = default(CancellationToken)) where T : class;
        IElasticClient ElasticClient { get; }
    }
}