using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Hawking.CSI.Forward.Services.ForwardQueueLockClients
{
    public class RedisForwardQueueLockClient : IForwardQueueLockClient
    {
        private readonly IConfiguration configuration;
        private readonly ILogger<RedisForwardQueueLockClient> logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly Uri redisUri;
        private readonly string redisHost;
        private readonly int redisPort;
        private readonly int redisDb;
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IServer redisServer;
        private IDatabase lockDb;

        public RedisForwardQueueLockClient(IConfiguration configuration, ILogger<RedisForwardQueueLockClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(RedisForwardQueueLockClient));
            redisUri = new Uri(serviceConfig["Uri"]);
            redisHost = redisUri.GetComponents(UriComponents.Host, UriFormat.UriEscaped);
            redisPort = int.Parse(redisUri.GetComponents(UriComponents.Port, UriFormat.UriEscaped));
            redisDb = int.Parse(redisUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped));

            var config = new ConfigurationOptions 
            { 
                EndPoints = { redisHost }, 
                AbortOnConnectFail = false
            };
            redisConnection = ConnectionMultiplexer.Connect(config);
            redisServer = redisConnection.GetServer(redisHost, redisPort);
            lockDb = redisConnection.GetDatabase(redisDb);
        }

        public async Task<bool> LockExistsAsync(string queueName, CancellationToken token)
        {
            return await lockDb.KeyExistsAsync(queueName);
        }

        public async Task<bool> TryGetLockAsync(string queueName, TimeSpan ttl, CancellationToken cancellationToken)
        {
            var result = await lockDb.StringSetAsync(queueName, RedisValue.EmptyString, expiry: ttl, when: When.NotExists);
            return result;
        }

        public async Task<bool> TryReleaseLockAsync(string queueName, CancellationToken cancellationToken)
        {
            return await lockDb.KeyDeleteAsync(queueName);
        }

        public async Task UpdateLockTtlAsync(string queueName, TimeSpan ttl, CancellationToken cancellationToken)
        {
            await lockDb.KeyExpireAsync(queueName, ttl);
        }
    }
}