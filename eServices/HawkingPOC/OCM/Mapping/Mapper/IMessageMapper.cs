using System.Threading.Tasks;
using OcmPoc.Infrastructure.MessageInterfaces.Queueing;

namespace OcmPoc.Mapping.Mapper
{
	interface IMessageMapper
    {
        Task<QueueItem> MapMessageAsync(QueueItem item);
    }
}
