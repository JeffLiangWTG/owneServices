using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Hawking.CSI.Forward.Services
{
    public interface IForwardHeartbeatClient
    {
        Task SetAsync(string hostName, TimeSpan heartbeatTTL, int taskCount, CancellationToken cancellationToken);
        Task PurgeExpiredHostsAsync(string hostName, CancellationToken cancellationToken);
        Task<IDictionary<string, int>> GetHostTaskCountsAsync(CancellationToken cancellationToken);
    }
}