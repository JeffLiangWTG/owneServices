using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Forward.Models
{
    public class SendQueueItem
    {
        public object Id { get; set; }
        public IDictionary<string, StringValues> MessageHeaders { get; set; }
        public Task<IDictionary<string, StringValues>> SenderTask { get; set; }
    }
}