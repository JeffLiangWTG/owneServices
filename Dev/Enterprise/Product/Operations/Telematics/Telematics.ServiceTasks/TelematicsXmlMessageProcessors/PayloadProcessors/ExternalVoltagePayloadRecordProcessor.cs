using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class ExternalVoltagePayloadRecordProcessor : IPayloadRecordProcessor<ExternalVoltagePayloadRecord>
	{
		public int Process(BusinessObjectFactory factory, GlbDevice device, ExternalVoltagePayloadRecord record)
		{
			var externalVoltage = factory.New<GlbDeviceExternalVoltage>();
			externalVoltage.GDV_V3_Device = device.PK;
			externalVoltage.GDV_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
			externalVoltage.GDV_Voltage = record.Voltage;
			return 1;
		}
	}
}
