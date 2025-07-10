using System;
using System.Linq;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class BatteryPayloadRecordProcessorTest : PayloadRecordProcessorTest<BatteryPayloadRecordProcessor, BatteryPayloadRecord>
	{
		public override void TestAddsRecord()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 12.34, 500, true, 12, 2.32);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.78, 900, false, 15, 5.71);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.78, 900, false, -15, 5.71);
			});

			void Test(DateTimeOffset dateTimeOffset, double chargeRemaining, int current, bool isCharging, int temperature, double voltage)
			{
				// Arrange
				var payloadRecord = new BatteryPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					ChargeRemaining = chargeRemaining,
					Current = current,
					IsCharging = isCharging,
					Temperature = temperature,
					Voltage = voltage,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.Batteries.Single();
				AssertEquals(expectedTime, result.GDB_MeasurementTimeUtc);
				AssertEquals((decimal)chargeRemaining, result.GDB_ChargeRemaining);
				AssertEquals(current / 1000.0M, result.GDB_CurrentA);
				AssertEquals(isCharging, result.GDB_IsCharging);
				AssertEquals(temperature, result.GDB_TemperatureC);
				AssertEquals((decimal)voltage, result.GDB_Voltage);
				result.Delete();
			}
		}
	}
}
