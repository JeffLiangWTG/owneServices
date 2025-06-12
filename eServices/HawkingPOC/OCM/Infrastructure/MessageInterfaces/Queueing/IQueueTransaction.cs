using System;

namespace OcmPoc.Infrastructure.MessageInterfaces.Queueing
{
    public interface IQueueTransaction : IDisposable
    {
		void Commit();
    }
}
