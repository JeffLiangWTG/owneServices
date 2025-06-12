using System;
using System.Collections.Generic;

namespace Hawking.CSI.Monitoring.Models
{
    public class TransactionMetrics
    {
        public Guid TrackingId { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public DateTime? Start { get; set; }
        public DateTime? End { get; set; }
        public double? Latency { get; set; }
        public double? ProcessingTime { get; set; }
        public long? SizeIn { get; set; }
        public long? SizeOut { get; set; }
        public Dictionary<string, TransactionEvent> Events { get; set; } 
    }

    public class TransactionEvent
    {
        public Guid TrackingId { get; set; }
        public string Sender { get; set; }
        public string Recipient { get; set; }
        public ulong Offset { get; set; }
        public string Type { get; set; }
        public DateTime Time { get; set; }
        public double? Latency { get; set; }
        public string StorageReference { get; set; }
        public long? Size { get; set; }
    }
}
