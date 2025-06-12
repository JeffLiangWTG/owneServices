using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Models;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Driver;
using Newtonsoft.Json;

namespace Hawking.CSI.Forward.Services.ForwardQueueClients
{
    public class MongoForwardQueueClient : IForwardQueueClient
    {
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly IMongoCollection<BsonDocument> collection;

        public MongoForwardQueueClient(IConfiguration configuration, ILogger<MongoForwardQueueClient> logger)
        {
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(MongoForwardQueueClient));
            var mongoUri = serviceConfig["Uri"];
            logger.LogInformation("Connecting to MongoDB server at {mongoUri}", mongoUri);

            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(serviceConfig["DatabaseName"]);
            collection = database.GetCollection<BsonDocument>(serviceConfig["CollectionName"]);
        }

        public async Task EnqueueAsync(IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken)
        {
            var document = messageHeaders.ToDictionary(h => h.Key, h => h.Value.ToArray()).ToBsonDocument();
            document.Add("Timestamp", DateTime.UtcNow);
            await collection.InsertOneAsync(document);
        }

        public async Task<SendQueueItem> GetNextAsync(string queueName, object afterRefId, CancellationToken cancellationToken)
        {
            var options = new FindOptions<BsonDocument>()
            {
                Sort = Builders<BsonDocument>.Sort.Ascending("_id"),
                Limit = 1
            };
            var filter = Builders<BsonDocument>.Filter.Eq("X-Forward-Queue", queueName);
            if (afterRefId != null)
            {
                filter = Builders<BsonDocument>.Filter.And(
                    Builders<BsonDocument>.Filter.Gt("_id", afterRefId),
                    filter
                );
            }

            var document = (await collection.FindAsync(filter, options)).FirstOrDefault();
            if (document == null)
                return null;

            var result = new SendQueueItem { Id = document["_id"].AsObjectId };
            document.Remove("_id");
            document.Remove("Timestamp");
            result.MessageHeaders = BsonSerializer.Deserialize<IDictionary<string, string[]>>(document)
                .ToDictionary(h => h.Key, h => (StringValues)h.Value);

            return result;
        }

        public async Task DeleteAsync(object refId, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", refId);
            await collection.DeleteOneAsync(filter);
        }

        public async Task<bool> AnyAsync(string queueName, CancellationToken cancellationToken)
        {
            return await Task.Run(() => collection.AsQueryable().Any(d => d["X-Forward-Queue"] == queueName));
        }

        public async Task<IEnumerable<string>> GetOlderAsync(TimeSpan before, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Lt("Timestamp", DateTime.UtcNow.Subtract(before));
            var names = await collection.DistinctAsync<string>("X-Forward-Queue", filter, cancellationToken: cancellationToken);
            return await names.ToListAsync();
        }
    }
}