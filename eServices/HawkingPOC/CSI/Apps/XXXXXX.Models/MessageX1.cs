using System;
using System.Collections.Generic;

namespace Hawking.CSI.Apps.XXXXXX.Models
{
    public class MessageX1
    {
        public Guid TrackingId { get; set; }
        public DateTime? CreatedUtc { get; set; }
        public List<string> Message { get; set; }
    }
}
