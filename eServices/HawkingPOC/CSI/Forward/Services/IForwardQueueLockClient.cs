using System;
using System.Threading;
using System.Threading.Tasks;

namespace Hawking.CSI.Forward.Services
{
    public interface IForwardQueueLockClient
    {
        Task<bool> TryGetLockAsync(string queueName, TimeSpan ttl, CancellationToken cancellationToken);
        Task UpdateLockTtlAsync(string queueName, TimeSpan ttl, CancellationToken cancellationToken);
        Task<bool> TryReleaseLockAsync(string queueName, CancellationToken cancellationToken);
        Task<bool> LockExistsAsync(string queueName, CancellationToken token);
    }
}