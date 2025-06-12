using System;
using System.Threading.Tasks;
using Hawking.CSI.Monitoring.Models;

namespace Hawking.CSI.Monitoring.Services
{
    public interface IEventStreamClient
    {
        Task SubscribeAsync(EventHandler<ConsumerEventArgs> consumerAction);
    }
}