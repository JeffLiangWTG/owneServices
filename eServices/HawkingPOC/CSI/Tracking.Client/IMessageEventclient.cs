using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Tracking.Client
{
    public interface IMessageEventClient
    {
        Task TrackMessageEventAsync(MessageEventType eventType, IDictionary<string, StringValues> messageHeaders, 
            CancellationToken cancellationToken);
        Task TrackMessageEventAsync(MessageEventType eventType, IDictionary<string, StringValues> eventHeaders, 
            IDictionary<string, StringValues> messageHeaders, CancellationToken cancellationToken);
    }
}
