using System;
using CargoWise.EntityFramework;
using Enterprise.Telematics.Business;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class BatteryPayloadRecordProcessor : IPayloadRecordProcessor<BatteryPayloadRecord>
	{
		public int Process(BusinessObjectFactory factory, GlbDevice device, BatteryPayloadRecord record)
		{
			var battery = factory.New<GlbDeviceBattery>();
			battery.GDB_V3_Device = device.PK;
			battery.GDB_MeasurementTimeUtc = record.DateTimeOffset.UtcDateTime;
			battery.GDB_CurrentA = record.Current / 1000.0;
			battery.GDB_ChargeRemaining = record.ChargeRemaining;
			battery.GDB_IsCharging = record.IsCharging;
			battery.GDB_TemperatureC = Convert.ToInt16(record.Temperature);
			battery.GDB_Voltage = record.Voltage;
			return 1;
		}
	}
}
