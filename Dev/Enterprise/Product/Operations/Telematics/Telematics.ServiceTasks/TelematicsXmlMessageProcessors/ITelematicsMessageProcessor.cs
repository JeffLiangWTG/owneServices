using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors
{
	interface ITelematicsMessageProcessor<T>
	{
		int Process(BusinessObjectFactory factory, T message);
	}
}
