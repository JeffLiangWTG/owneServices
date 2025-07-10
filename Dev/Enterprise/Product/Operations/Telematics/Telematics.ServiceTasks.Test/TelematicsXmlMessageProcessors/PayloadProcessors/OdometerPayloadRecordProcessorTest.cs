using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Moq;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class OdometerPayloadRecordProcessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_MobileServicesIdentifier = Array.Empty<byte>();
			device.V3_HardwareIdentifier = "TT00000001";
			device.V3_Model = string.Empty;

			deviceForConstraints = Factory.New<GlbDevice>();
			deviceForConstraints.V3_HumanReadableIdentifier = "TT00000002";
			deviceForConstraints.V3_IsActive = true;
			deviceForConstraints.V3_MobileServicesIdentifier = Array.Empty<byte>();
			deviceForConstraints.V3_HardwareIdentifier = "TT00000002";
			deviceForConstraints.V3_Model = string.Empty;

			initialData = new List<(ZDateTime, ZDecimal)>() {
				((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
				((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
				((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
				((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
				((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
			};

			loggerMock = new Mock<ILogger>();
			processor = new OdometerPayloadRecordProcessor(loggerMock.Object);
		}

		public void TestAddsRecord()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 12.34);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.78);
			});

			void Test(DateTimeOffset dateTimeOffset, double odometer)
			{
				// Arrange
				var payloadRecord = new OdometerPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					Odometer = odometer,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.Odometers.Single();
				AssertEquals(expectedTime, result.GDO_MeasurementTimeUtc);
				AssertEquals((decimal)odometer, result.GDO_OdometerKM);
				result.Delete();
			}
		}

		public void TestRecordsThatDoNotViolateConstraintsAreAdded()
		{
			CombineAssertions(() =>
			{
				GenerateDeviceOdometersAndExpectedResult("TestRecordsThatDoNotViolateConstraintsAreAddedHardwaerId1");
				ValidRecordTest(
					dateTimeE.AddDays(1),
					odometerE + 10,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE),
						((ZDateTime)dateTimeE.AddDays(1).UtcDateTime, (ZDecimal)(odometerE + 10))
				});
			});
		}

		public void TestRecordsWithSameOdometerAndEarlierTimeUpdateExistingRecords()
		{
			CombineAssertions(() =>
			{
				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndEarlierTimeUpdateExistingReocrdsHardwareId1");
				ValidRecordTest(
					dateTimeA.AddDays(-1),
					odometerA,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.AddDays(-1).UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndEarlierTimeUpdateExistingReocrdsHardwareId2");
				ValidRecordTest(
					dateTimeE.AddHours(-1),
					odometerE,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.AddHours(-1).UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndEarlierTimeUpdateExistingReocrdsHardwareId3");
				ValidRecordTest(
					dateTimeC.AddHours(-2),
					odometerC,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.AddHours(-2).UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});
			});
		}

		public void TestRecordsWithSameOdometerAndLaterTimeDiscarded()
		{
			CombineAssertions(() =>
			{
				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndLaterTimeDiscarded1");
				ValidRecordTest(
					dateTimeA.AddDays(1),
					odometerA,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndLaterTimeDiscarded2");
				ValidRecordTest(
					dateTimeA,
					odometerA,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndLaterTimeDiscarded3");
				ValidRecordTest(
					dateTimeE.AddDays(1),
					odometerE,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndLaterTimeDiscarded4");
				ValidRecordTest(
					dateTimeE,
					odometerE,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndLaterTimeDiscarded5");
				ValidRecordTest(
					dateTimeC.AddHours(2),
					odometerC,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndLaterTimeDiscarded6");
				ValidRecordTest(
					dateTimeC,
					odometerC,
					new List<(ZDateTime, ZDecimal)>() {
						((ZDateTime)dateTimeA.UtcDateTime, (ZDecimal)odometerA),
						((ZDateTime)dateTimeB.UtcDateTime, (ZDecimal)odometerB),
						((ZDateTime)dateTimeC.UtcDateTime, (ZDecimal)odometerC),
						((ZDateTime)dateTimeD.UtcDateTime, (ZDecimal)odometerD),
						((ZDateTime)dateTimeE.UtcDateTime, (ZDecimal)odometerE)
				});
			});
		}

		public void TestRecordsWithSameOdometerAndTimeEqualOrEarlierThanPreviousRecordsDiscarded()
		{
			CombineAssertions(() =>
			{
				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndTimeEqualOrEarlierThanPreviousRecordsDiscarded1");
				Test(dateTimeD, odometerE);

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndTimeEqualOrEarlierThanPreviousRecordsDiscarded2");
				Test(dateTimeD.AddHours(-2), odometerE);

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndTimeEqualOrEarlierThanPreviousRecordsDiscarded3");
				Test(dateTimeB, odometerC);

				GenerateDeviceOdometersAndExpectedResult("TestRecordsWithSameOdometerAndTimeEqualOrEarlierThanPreviousRecordsDiscarded4");
				Test(dateTimeB.AddHours(-2), odometerC);
			});

			void Test(DateTimeOffset dateTimeOffset, double odometer)
			{
				// Arrange
				var payloadRecord = new OdometerPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					Odometer = odometer,
				};

				// Act
				processor.Process(Factory, deviceForConstraints, payloadRecord);

				// Assert
				AssertNoExceptionThrown(() =>
				{
					loggerMock.Verify(logger => logger.Log(LogType.Warning, $"Odometer record at time: {payloadRecord.DateTimeOffset.ToString()} with value: {payloadRecord.Odometer} was invalid"));
				});
			}
		}

		public void TestWrongConstructorParamsCall()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new OdometerPayloadRecordProcessor(null));
			AssertEquals("logger", result.ParamName);
		}

		void GenerateDeviceOdometersAndExpectedResult(string hardwareIdentifier)
		{
			deviceForConstraints = Factory.New<GlbDevice>();
			deviceForConstraints.V3_HumanReadableIdentifier = hardwareIdentifier;
			deviceForConstraints.V3_IsActive = true;
			deviceForConstraints.V3_MobileServicesIdentifier = Array.Empty<byte>();
			deviceForConstraints.V3_HardwareIdentifier = hardwareIdentifier;
			deviceForConstraints.V3_Model = string.Empty;

			foreach (var data in initialData)
			{
				var odometer = Factory.New<GlbDeviceOdometer>();
				odometer.GDO_V3_Device = deviceForConstraints.PK;
				odometer.GDO_MeasurementTimeUtc = data.Item1;
				odometer.GDO_OdometerKM = data.Item2;
			}
		}

		void ValidRecordTest(DateTimeOffset dateTimeOffset, double odometer, List<(ZDateTime, ZDecimal)> expectedResult)
		{
			// Arrange
			var payloadRecord = new OdometerPayloadRecord
			{
				DateTimeOffset = dateTimeOffset,
				Odometer = odometer,
			};

			// Act
			processor.Process(Factory, deviceForConstraints, payloadRecord);

			// Assert
			AssertContainsExactElementsInAnyOrder(deviceForConstraints.Odometers.Select(odo => (odo.GDO_MeasurementTimeUtc, odo.GDO_OdometerKM)), expectedResult);
		}

		OdometerPayloadRecordProcessor processor;
		GlbDevice device;
		GlbDevice deviceForConstraints;
		Mock<ILogger> loggerMock;

		readonly DateTimeOffset dateTimeA = new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero);
		readonly DateTimeOffset dateTimeB = new DateTimeOffset(2012, 5, 7, 10, 5, 23, TimeSpan.Zero);
		readonly DateTimeOffset dateTimeC = new DateTimeOffset(2012, 5, 8, 10, 5, 23, TimeSpan.Zero);
		readonly DateTimeOffset dateTimeD = new DateTimeOffset(2012, 5, 9, 10, 5, 23, TimeSpan.Zero);
		readonly DateTimeOffset dateTimeE = new DateTimeOffset(2012, 5, 10, 10, 5, 23, TimeSpan.Zero);
		readonly double odometerA = 56.78;
		readonly double odometerB = 78.83;
		readonly double odometerC = 96.78;
		readonly double odometerD = 102.89;
		readonly double odometerE = 125.20;
		List<(ZDateTime, ZDecimal)> initialData = new List<(ZDateTime, ZDecimal)>();
	}
}
