using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Models;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Services
{
    public interface IForwardQueueClient
    {
        Task EnqueueAsync(IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken);
        Task<SendQueueItem> GetNextAsync(string queueName, object afterId, CancellationToken cancellationToken);
        Task DeleteAsync(object refId, CancellationToken cancellationToken);
        Task<bool> AnyAsync(string queueName, CancellationToken cancellationToken);
        Task<IEnumerable<string>> GetOlderAsync(TimeSpan before, CancellationToken cancellationToken);
    }
}