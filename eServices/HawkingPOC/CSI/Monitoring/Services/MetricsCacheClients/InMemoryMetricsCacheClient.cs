using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace Hawking.CSI.Monitoring.Services.MetricsCacheClients
{
    public class InMemoryMetricsCacheClient : IMetricsCacheClient
    {
        private readonly ILogger logger;

        private readonly ConcurrentDictionary<ulong, TransactionEvent> eventCache =
            new ConcurrentDictionary<ulong, TransactionEvent>();

        public InMemoryMetricsCacheClient(ILogger<InMemoryMetricsCacheClient> logger)
        {
            this.logger = logger;
        }

        public Task AddAsync(TransactionEvent tranEvent)
        {
            eventCache.TryAdd(tranEvent.Offset, tranEvent);
            return Task.CompletedTask;
        }

        public Task<TransactionMetrics> GetAsync(Guid trackingId)
        {
            var events = eventCache.Values.Where(e => e.TrackingId == trackingId).ToList();
            events.Sort((a, b) => a.Offset.CompareTo(b.Offset));

            var transactionMetrics = new TransactionMetrics 
            {
                TrackingId = trackingId,
                Sender = events.First().Sender,
                Recipient = events.First().Recipient,
                Start = events.First().Time,
                End = events.Last().Time,
                Latency = events.Last().Time.Subtract(events.First().Time).TotalMilliseconds,
                SizeIn = events.First().Size,
                SizeOut = events.Last().Size,
                Events = new Dictionary<string, TransactionEvent>()
            };

            double processingTime = 0;

            for (int i = 0; i < events.Count; i++)
            {
                if (i > 0)
                {
                    events[i].Latency = events[i].Time.Subtract(events[i - 1].Time).TotalMilliseconds;
                }
                switch (events[i].Type)
                {
                    case "Received":
                        break;
                    case "ForwardIn":
                        processingTime += events[i].Latency.Value;
                        break;
                    case "ForwardOut":
                        break;
                    case "Processing":
                        processingTime += events[i].Latency.Value;
                        break;
                    case "Processed":
                        processingTime += events[i].Latency.Value;
                        break;
                    case "ForwardContinue":
                        break;
                    case "Sending":
                        processingTime += events[i].Latency.Value;
                        break;
                    case "Sent":
                        processingTime += events[i].Latency.Value;
                        break;
                }
                transactionMetrics.Events.Add(i.ToString(), events[i]);
                eventCache.TryRemove(events[i].Offset, out TransactionEvent removedEvent);
            }
            transactionMetrics.ProcessingTime = processingTime;

            return Task.FromResult(transactionMetrics);
        }
    }
}