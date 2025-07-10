using System;
using System.Linq;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class ExternalVoltagePayloadRecordProcessorTest : PayloadRecordProcessorTest<ExternalVoltagePayloadRecordProcessor, ExternalVoltagePayloadRecord>
	{
		public override void TestAddsRecord()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 2.32);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 5.71);
			});

			void Test(DateTimeOffset dateTimeOffset, double voltage)
			{
				// Arrange
				var payloadRecord = new ExternalVoltagePayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					Voltage = voltage,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.ExternalVoltages.Single();
				AssertEquals(expectedTime, result.GDV_MeasurementTimeUtc);
				AssertEquals((decimal)voltage, result.GDV_Voltage);
				result.Delete();
			}
		}
	}
}
