using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	public interface ITelematicsXmlMessageProcessor
	{
		int Process(BusinessObjectFactory factory, string messageText);
	}
}
