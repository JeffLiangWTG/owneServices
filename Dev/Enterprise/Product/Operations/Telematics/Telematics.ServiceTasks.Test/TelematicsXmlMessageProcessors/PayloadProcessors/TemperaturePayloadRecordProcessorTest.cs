using System;
using System.Linq;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class TemperaturePayloadRecordProcessorTest : PayloadRecordProcessorTest<TemperaturePayloadRecordProcessor, TemperaturePayloadRecord>
	{
		public override void TestAddsRecord()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 12.34);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.78);
			});

			void Test(DateTimeOffset dateTimeOffset, double temperature)
			{
				// Arrange
				var payloadRecord = new TemperaturePayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					Temperature = temperature,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.Temperatures.Single();
				AssertEquals(expectedTime, result.GDT_MeasurementTimeUtc);
				AssertEquals((decimal)temperature, result.GDT_TemperatureC);
				result.Delete();
			}
		}
	}
}
