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
    public class GridFsObjectStoreClient : IObjectStoreClient
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly GridFSBucket gridFs;

        public GridFsObjectStoreClient(IConfiguration configuration, ILogger<GridFsObjectStoreClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(GridFsObjectStoreClient));
            var mongoUri = serviceConfig["Uri"];
            logger.LogInformation("Connecting to MongoDB server at {mongoUri}", mongoUri);

            var client = new MongoClient(mongoUri);
            var database = client.GetDatabase(serviceConfig["DatabaseName"]);
            var options = new GridFSBucketOptions { BucketName = serviceConfig["BucketName"], DisableMD5 = true };
            gridFs = new GridFSBucket(database, options);
        }

        public async Task<Stream> ReadObjectAsync(string id, CancellationToken cancellationToken)
        {
            return await gridFs.OpenDownloadStreamAsync(ObjectId.Parse(id));
        }

        public async Task<string> WriteObjectAsync(string reference, Stream body, CancellationToken cancellationToken)
        {
            return (await gridFs.UploadFromStreamAsync(reference, body)).ToString();
        }
    }
}