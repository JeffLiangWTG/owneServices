using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using WTG.Telematics.Interfaces.Packets.V2;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	interface IPayloadRecordProcessor<T> where T : IPayloadRecord
	{
		int Process(BusinessObjectFactory factory, GlbDevice device, T record);
	}
}
