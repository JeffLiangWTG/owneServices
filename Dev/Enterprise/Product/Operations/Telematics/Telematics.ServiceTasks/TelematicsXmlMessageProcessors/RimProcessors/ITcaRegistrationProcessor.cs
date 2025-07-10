using CargoWise.EntityFramework;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors
{
	public interface ITcaRegistrationProcessor<T> where T : class
	{
		int Process(BusinessObjectFactory factory, T message);
	}
}
