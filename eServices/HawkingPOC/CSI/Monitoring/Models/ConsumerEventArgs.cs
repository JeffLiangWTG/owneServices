using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Hawking.CSI.Monitoring.Models
{
    public class ConsumerEventArgs : EventArgs
    {
        public ConsumerEventArgs(DateTime timeStamp, ulong sequence, string source, byte[] data, Action ack)
        {
            TimeStamp = timeStamp;
            Sequence = sequence;
            Source = source;
            Data = data;
            Ack = ack;
        }

        public DateTime TimeStamp { get; }
        public ulong Sequence { get; }
        public string Source { get; }
        public byte[] Data { get; }
        public Action Ack { get; }
    }
}
