using System;
using System.Collections.Generic;

namespace xHub.DatImplementation.ContainerDeployment
{
    public class LoadBalancer
    {
        private readonly List<Server> _servers;
        private readonly Random _random;

        public LoadBalancer(ServerSettings serverSettings)
        {
            _servers = serverSettings.Servers ?? throw new ArgumentNullException(nameof(serverSettings.Servers));
            if (_servers.Count == 0)
            {
                throw new InvalidOperationException("Server list cannot be empty.");
            }
            _random = new Random();
        }

        public (string errorMessage, Server selectedServer) SelectRandomServerInstance(List<Server> excludedServers)
        {
            try
            {
                excludedServers ??= new List<Server>();
                var availableServers = _servers
                    .Where(server => !excludedServers.Contains(server))
                    .ToList();

                if (!availableServers.Any())
                {
                    return ("No available servers after applying exclusions.", new());
                }
                var selectedServer = availableServers[_random.Next(availableServers.Count)];

                return (string.Empty, selectedServer);
            }
            catch (Exception ex)
            {
                return (ex.Message??"Invalid Operation", new());
            }
            
        }
    }
}