using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MongoDB.Bson;
using MongoDB.Driver;
using MongoDB.Driver.GridFS;

namespace Hawking.CSI.ContentStore.Services
{
    public class MongoObjectStoreClient : IObjectStoreClient
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly GridFSBucket gridFs;
        private readonly IMongoCollection<BsonDocument> collection;

        public MongoObjectStoreClient(IConfiguration configuration, ILogger<MongoObjectStoreClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(MongoObjectStoreClient));
            var mongoUri = serviceConfig["Uri"];
            logger.LogInformation("Connecting to MongoDB server at {mongoUri}", mongoUri);

            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(serviceConfig["DatabaseName"]);
            collection = database.GetCollection<BsonDocument>(serviceConfig["CollectionName"]);
        }

        public async Task<Stream> ReadObjectAsync(string id, CancellationToken cancellationToken)
        {
            var filter = Builders<BsonDocument>.Filter.Eq("_id", new ObjectId(id));
            var document = (await collection.FindAsync(filter)).FirstOrDefault();
            if (document == null)
                return null;
            return new MemoryStream(document["message"].AsByteArray);
        }

        public async Task<string> WriteObjectAsync(string reference, Stream body, CancellationToken cancellationToken)
        {
            using (var ms = new MemoryStream())
            {
                await body.CopyToAsync(ms);
                var objectValue = ms.ToArray();
                var document = new 
                { 
                    id = ObjectId.Empty,
                    message = objectValue
                }.ToBsonDocument();
                await collection.InsertOneAsync(document);
                var objectId = document["_id"].AsObjectId;
                return objectId.ToString();
            }
        }
    }
}