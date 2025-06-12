using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StackExchange.Redis;

namespace Hawking.CSI.Forward.Services.ForwardHeartbeatClients
{
    public class RedisForwardHeartbeatClient : IForwardHeartbeatClient
    {
        private readonly IConfiguration configuration;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly Uri redisUri;
        private readonly string redisHost;
        private readonly int redisPort;
        private readonly int redisDb;
        private readonly ConnectionMultiplexer redisConnection;
        private readonly IServer redisServer;
        private readonly IDatabase heartbeatDb;
        private const string LOADBALANCINGKEY = "LB-HOSTS";
        private const string HEARTBBEATPREFIX = "HB-";

        public RedisForwardHeartbeatClient(IConfiguration configuration, ILogger<RedisForwardHeartbeatClient> logger)
        {
            this.configuration = configuration;
            this.logger = logger;

            serviceConfig = configuration.GetSection(nameof(RedisForwardHeartbeatClient));
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
            heartbeatDb = redisConnection.GetDatabase(redisDb);
        }

        public async Task SetAsync(string hostName, TimeSpan heartbeatTTL, int taskCount, CancellationToken cancellationToken)
        {
            await heartbeatDb.StringSetAsync($"{HEARTBBEATPREFIX}{hostName}", RedisValue.EmptyString, heartbeatTTL);
            await heartbeatDb.HashSetAsync(LOADBALANCINGKEY, new[] { new HashEntry(hostName, taskCount) });
        }

        public async Task PurgeExpiredHostsAsync(string hostName, CancellationToken cancellationToken)
        {
            var entries = await heartbeatDb.HashKeysAsync(LOADBALANCINGKEY);
            foreach (var entry in entries)
            {
                if (entry != hostName)
                {
                    if (!await heartbeatDb.KeyExistsAsync($"{HEARTBBEATPREFIX}{entry}"))
                    {
                        logger.LogDebug("Purging expired host {HostName} from load-balancing", entry);
                        await heartbeatDb.HashDeleteAsync(LOADBALANCINGKEY, entry);
                    }
                }
            }
        }

        public async Task<IDictionary<string, int>> GetHostTaskCountsAsync(CancellationToken cancellationToken)
        {
            var entries = await heartbeatDb.HashGetAllAsync(LOADBALANCINGKEY);
            return entries.ToDictionary(h => h.Name.ToString(), h => int.Parse(h.Value));
        }
    }
}