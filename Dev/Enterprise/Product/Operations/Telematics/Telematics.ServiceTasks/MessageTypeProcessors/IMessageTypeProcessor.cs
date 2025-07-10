using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.MessageTypeProcessors
{
	public interface IMessageTypeProcessor
	{
		string MessageType { get; }
		int Process(BusinessObjectFactory factory, string from, string messageText);
	}
}
