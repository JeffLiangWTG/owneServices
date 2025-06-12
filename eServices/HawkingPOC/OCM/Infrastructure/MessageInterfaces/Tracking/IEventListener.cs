using System;
using OcmPoc.Infrastructure.MessageInterfaces.Entities;

namespace OcmPoc.Infrastructure.MessageInterfaces.Tracking
{
	public interface IEventListener
	{
		event EventHandler<string> OnLog;
		IObservable<MessageEvent> Events { get; }
	}
}
