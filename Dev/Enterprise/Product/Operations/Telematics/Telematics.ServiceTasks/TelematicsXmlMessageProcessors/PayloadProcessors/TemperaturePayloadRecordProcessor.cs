using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class TemperaturePayloadRecordProcessor : IPayloadRecordProcessor<TemperaturePayloadRecord>
	{
		public int Process(BusinessObjectFactory factory, GlbDevice device, TemperaturePayloadRecord record)
		{
			var temperature = factory.New<GlbDeviceTemperature>();
			temperature.GDT_V3_Device = device.PK;
			temperature.GDT_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
			temperature.GDT_TemperatureC = record.Temperature;
			return 1;
		}
	}
}
