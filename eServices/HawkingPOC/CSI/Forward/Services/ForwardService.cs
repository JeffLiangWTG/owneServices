using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Hawking.CSI.Forward.Models;
using Hawking.CSI.Tracking.Client;
using Hawking.CSI.Utilities;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Services
{
    public class ForwardService : IHostedService
    {
        private readonly ConcurrentDictionary<string, Task> forwarders = new ConcurrentDictionary<string, Task>();
        private readonly CancellationTokenSource shutdown = new CancellationTokenSource();
        private readonly IForwardEnqueueService forwardEnqueueService;
        private readonly IForwardQueueClient forwardQueueClient;
        private readonly IForwardQueueLockClient forwardQueueLockClient;
        private readonly IForwardHeartbeatClient forwardHeartbeatClient;
        private readonly IApplicationRegistryClient applicationRegistryClient;
        private readonly IServiceProvider serviceProvider;
        private readonly IMessageEventClient messageEventClient;
        private readonly ILogger logger;
        private readonly IConfigurationSection serviceConfig;
        private readonly string hostName;
        private readonly TimeSpan hostHeartbeatInterval;
        private readonly TimeSpan hostHeartbeatTTL;
        private readonly TimeSpan queueLockHeartbeatInterval;
        private readonly TimeSpan queueLockHeartbeatTtl;
        private Task signalReceiverTask;
        private Task hostHeartbeatTask;
        private Task orphanedQueueMonitorTask;

        public ForwardService(IForwardEnqueueService forwardEnqueueService,
            IForwardQueueClient forwardQueueClient,
            IForwardQueueLockClient forwardQueueLockClient,
            IForwardHeartbeatClient forwardHeartbeatClient,
            IApplicationRegistryClient applicationRegistryClient,
            IServiceProvider serviceProvider,
            IMessageEventClient messageEventClient,
            IConfiguration configuration,
            ILogger<ForwardService> logger)
        {
            this.forwardEnqueueService = forwardEnqueueService;
            this.forwardQueueClient = forwardQueueClient;
            this.forwardQueueLockClient = forwardQueueLockClient;
            this.forwardHeartbeatClient = forwardHeartbeatClient;
            this.applicationRegistryClient = applicationRegistryClient;
            this.serviceProvider = serviceProvider;
            this.messageEventClient = messageEventClient;
            this.logger = logger;
            serviceConfig = configuration.GetSection(nameof(ForwardService));
            hostName = configuration["HOSTNAME"] ?? Environment.MachineName;
            hostHeartbeatInterval = TimeSpan.FromSeconds(double.Parse(serviceConfig["HostHeartbeatInterval"]));
            hostHeartbeatTTL = TimeSpan.FromSeconds(double.Parse(serviceConfig["HostHeartbeatTTL"]));
            queueLockHeartbeatInterval = TimeSpan.FromSeconds(double.Parse(serviceConfig["QueueLockHeartbeatInterval"]));
            queueLockHeartbeatTtl = TimeSpan.FromSeconds(double.Parse(serviceConfig["QueueLockHeartbeatTTL"]));
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            signalReceiverTask = SignalReceiver();
            hostHeartbeatTask = HostHeartbeat();
            orphanedQueueMonitorTask = OrphanedQueueMonitor();
            return Task.CompletedTask;
        }

        private async Task HostHeartbeat()
        {
            while (!shutdown.IsCancellationRequested)
            {
                try
                {
                    await forwardHeartbeatClient.SetAsync(hostName, hostHeartbeatTTL, forwarders.Count, shutdown.Token);
                    await forwardHeartbeatClient.PurgeExpiredHostsAsync(hostName, shutdown.Token);
                    await Task.Delay(hostHeartbeatInterval);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error performing heartbeat");
                }
            }
        }

        private async Task OrphanedQueueMonitor()
        {
            while (!shutdown.IsCancellationRequested)
            {
                try
                {
                    var hostTaskCounts = await forwardHeartbeatClient.GetHostTaskCountsAsync(shutdown.Token);
                    if (!hostTaskCounts.Any() || hostTaskCounts.First(h => h.Value == hostTaskCounts.Values.Min()).Key == hostName)
                    {
                        var agingQueues = await forwardQueueClient.GetOlderAsync(queueLockHeartbeatTtl, shutdown.Token);
                        foreach(var queueName in agingQueues)
                        {
                            if (!await forwardQueueLockClient.LockExistsAsync(queueName, shutdown.Token))    
                            {
                                logger.LogDebug("Restarting aged queue {QueueName}", queueName);
                                await forwardEnqueueService.EnqueueSignalAsync(queueName, shutdown.Token);
                            }
                        }
                    }
                    await Task.Delay(queueLockHeartbeatInterval);
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error performing heartbeat");
                }
            }
        }

        private async Task SignalReceiver()
        {
            while (!shutdown.IsCancellationRequested)
            {
                try
                {
                    var queueName = await forwardEnqueueService.DequeueSignalAsync(shutdown.Token);
                    logger.LogDebug("Signal received for {QueueName}", queueName);

                    if (!forwarders.ContainsKey(queueName))
                    {
                        if (await forwardQueueLockClient.TryGetLockAsync(queueName, queueLockHeartbeatTtl, shutdown.Token))
                        {
                            logger.LogDebug("Starting forwarder for {QueueName}", queueName);
                            var destinationAddress = ApplicationAddress.Parse(queueName.Remove(queueName.IndexOf('|')));
                            var applicationInfo = await applicationRegistryClient.GetApplicationAsync(destinationAddress, shutdown.Token);
                            forwarders[queueName] = ForwarderTaskAsync(queueName, applicationInfo, shutdown.Token)
                                .ContinueWith(async (t, q) =>
                                {
                                    logger.LogDebug("Stopping queue {QueueName}", q);
                                    while (!await forwardQueueLockClient.TryReleaseLockAsync((string)q, shutdown.Token)) { }
                                    while (!forwarders.TryRemove((string)q, out Task value)) { }
                                    logger.LogDebug("Stopped queue {QueueName}", q);
                                    if (await forwardQueueClient.AnyAsync(queueName, shutdown.Token))
                                        await forwardEnqueueService.EnqueueSignalAsync(queueName, shutdown.Token);
                                }, queueName, shutdown.Token);
                        }
                    }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error receiving forward signals");
                }
            }
        }

        private async Task ForwarderTaskAsync(string queueName, 
            ApplicationInfo applicationInfo, 
            CancellationToken cancellationToken)
        {
            var queueStop = new CancellationTokenSource();

            try
            {
                object lastInProcessId = null;
                var parallelizer = new SemaphoreSlim(applicationInfo.Parallelism);
                var senderQueue = new ConcurrentQueue<SendQueueItem>();
                var continueSignal = new SemaphoreSlim(0);
                var continueTask = ContinueTask(queueName, continueSignal, senderQueue, queueStop.Token);
                var heartbeatTask = QueueLockHeartbeatTask(queueName, queueStop.Token);

                using (var serviceScope = serviceProvider.CreateScope())
                {
                    var applicationClient = serviceScope.ServiceProvider.GetService<IApplicationClient>();
                    applicationClient.Uri = applicationInfo.Uri;

                    while (!cancellationToken.IsCancellationRequested)
                    {
                        await Task.WhenAny(
                            parallelizer.WaitAsync(),
                            Task.Delay(Timeout.Infinite, cancellationToken)
                        );
                        if (cancellationToken.IsCancellationRequested)
                            break;

                        var queueItem = await forwardQueueClient.GetNextAsync(queueName, lastInProcessId, cancellationToken);
                        if (queueItem == null)
                        {
                            parallelizer.Release();
                            if (senderQueue.Count == 0)
                                break;
                            
                            await Task.Delay(1);
                            continue;
                        }

                        logger.LogDebug("Starting new task for queue {QueueName} with remaining parallel worker slots: {Count}", queueName, parallelizer.CurrentCount);
                        logger.LogTrace("Task queue item: {@QueueItem}", queueItem);
                        lastInProcessId = queueItem.Id;
                        queueItem.SenderTask = SendMessageAsync(queueItem, applicationClient, cancellationToken)
                            .ContinueWith(t => 
                            {
                                logger.LogDebug("Releasing worker slot for {QueueName}", queueName);
                                parallelizer.Release(); 
                                return t.Result; 
                            }, cancellationToken);
                        senderQueue.Enqueue(queueItem);
                        continueSignal.Release();
                    }
                }
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error processing queue {QueueName}", queueName);
            }
            finally
            {
                queueStop.Cancel();
            }
        }

        private async Task QueueLockHeartbeatTask(string queueName, CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await forwardQueueLockClient.UpdateLockTtlAsync(queueName, queueLockHeartbeatTtl, cancellationToken);
                await Task.Delay(queueLockHeartbeatInterval, cancellationToken);
            }
        }

        private async Task<IDictionary<string, StringValues>> SendMessageAsync(SendQueueItem queueItem, 
            IApplicationClient applicationClient, 
            CancellationToken cancellationToken)
        {
            try
            {
                await messageEventClient.TrackMessageEventAsync(MessageEventType.ForwardOut, queueItem.MessageHeaders, cancellationToken);
                logger.LogDebug("Sending message to {Uri}", applicationClient.Uri);
                return await applicationClient.SendAsync(queueItem.MessageHeaders, cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error sending message with tracking ID {TrackingId}", queueItem.MessageHeaders["X-TrackingId"]);
                throw;
            }        
        }

        private async Task ContinueTask(string queueName, SemaphoreSlim continueSignal, 
            ConcurrentQueue<SendQueueItem> sendQueue, 
            CancellationToken cancellationToken)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                try
                {
                    if (sendQueue.Count == 0)
                    {
                        await Task.WhenAny(
                            continueSignal.WaitAsync(),
                            Task.Delay(Timeout.Infinite, cancellationToken)
                        );
                    }
                    if (cancellationToken.IsCancellationRequested)
                        return;

                    if (!sendQueue.TryPeek(out SendQueueItem sendItem))
                        continue;

                    logger.LogDebug("Starting continue task for queue {QueueName}", queueName);
                    logger.LogTrace("Continuation task item: {@TaskItem}", sendItem);

                    var responseHeaders = await sendItem.SenderTask;
                    if (responseHeaders.TryGetValue("X-Forward", out StringValues value)
                        && bool.TryParse(value, out bool forward) && forward)
                    {
                        await messageEventClient.TrackMessageEventAsync(MessageEventType.ForwardContinue, responseHeaders, cancellationToken);
                        responseHeaders.Remove("X-Forward");
                        var destinationAddress = ApplicationAddress.Parse(responseHeaders["X-Destination-Address"]);
                        logger.LogDebug("Forwarding response for queue {QueueName} to destination {Destination}", queueName, destinationAddress);
                        await forwardEnqueueService.EnqueueMessageAsync(destinationAddress, responseHeaders, cancellationToken);
                    }

                    await forwardQueueClient.DeleteAsync(sendItem.Id, cancellationToken);
                    while (!sendQueue.TryDequeue(out sendItem)) { }
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Error continuing sent message tasks for queue {QueueName}", queueName);
                    throw;
                }
            }
        }

        public Task StopAsync(CancellationToken cancellationToken)
        {
            shutdown.Cancel();
            return Task.WhenAny(
                Task.WhenAll(signalReceiverTask, Task.WhenAll(forwarders.Values)),
                Task.Delay(Timeout.Infinite, cancellationToken)
            );
        }
    }
}
