using CargoWise.EntityFramework;
using CargoWise.MobileServices.Common.Messages.EHub;

namespace Enterprise.Telematics.ServiceTasks.MessageProcessing
{
	public interface IMessageProcessor
	{
		EHubMessageType MessageType { get; }
		void Process(BusinessObjectFactory factory, string from, EHubMessageContainer messageContainer);
	}
}
