using System;
using System.Threading.Tasks;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
    public interface IQueueWriter : IDisposable
	{
		IQueueTransaction NewTransaction();
		void Publish(QueueItem item);
    }
}
