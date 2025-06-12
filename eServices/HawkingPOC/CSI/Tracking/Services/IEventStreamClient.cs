using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Primitives;

namespace Hawking.CSI.Tracking.Services
{
    public interface IEventStreamClient
    {
        Task PublishEventAsync(IDictionary<string, StringValues> messageHeaders);
    }
}