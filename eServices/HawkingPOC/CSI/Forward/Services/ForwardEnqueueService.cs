using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Services
{
    public class ForwardEnqueueService : IForwardEnqueueService
    {
        private readonly ConcurrentQueue<string> forwardSignalQueue = new ConcurrentQueue<string>();
        private readonly SemaphoreSlim dequeueSignal = new SemaphoreSlim(0);
        private readonly IForwardQueueClient forwardQueueClient;
        private readonly IForwardHeartbeatClient forwardHeartbeatClient;
        private readonly IMessageEventClient messageEventClient;
        private readonly IConfiguration configuration;
        private readonly string hostName;

        public ForwardEnqueueService(IForwardQueueClient forwardQueueClient, IForwardHeartbeatClient forwardHeartbeatClient, IMessageEventClient messageEventClient, IConfiguration configuration)
        {
            this.forwardQueueClient = forwardQueueClient;
            this.forwardHeartbeatClient = forwardHeartbeatClient;
            this.messageEventClient = messageEventClient;
            this.configuration = configuration;
            hostName = configuration["HOSTNAME"] ?? Environment.MachineName;
        }

        public async Task EnqueueMessageAsync(ApplicationAddress address, IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken)
        {
            var forwardQueue = $"{address.ApplicationName}|{messageHeaders["X-Sender"]}";
            messageHeaders["X-Destination-Address"] = address;
            messageHeaders["X-Forward-Queue"] = forwardQueue;
            await forwardQueueClient.EnqueueAsync(messageHeaders, cancellationToken);
            await messageEventClient.TrackMessageEventAsync(MessageEventType.ForwardIn, messageHeaders, cancellationToken);

            var forwardTaskCounts = await forwardHeartbeatClient.GetHostTaskCountsAsync(cancellationToken);
            var avgCount = forwardTaskCounts.Values.Average();
            if (forwardTaskCounts[hostName] <= avgCount)
                await EnqueueSignalAsync(forwardQueue, cancellationToken);
        }

        public Task EnqueueSignalAsync(string queueName, CancellationToken cancellationToken)
        {
            forwardSignalQueue.Enqueue(queueName);
            dequeueSignal.Release();
            return Task.CompletedTask;
        }

        public async Task<string> DequeueSignalAsync(CancellationToken cancellationToken)
        {
            await dequeueSignal.WaitAsync(cancellationToken);
            forwardSignalQueue.TryDequeue(out var queueName);
            return queueName;
        }
    }

    public interface IForwardEnqueueService
    {
        Task EnqueueMessageAsync(ApplicationAddress address, IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken);
        Task EnqueueSignalAsync(string queueName, CancellationToken cancellationToken);
        Task<string> DequeueSignalAsync(CancellationToken cancellationToken);
    }
}
