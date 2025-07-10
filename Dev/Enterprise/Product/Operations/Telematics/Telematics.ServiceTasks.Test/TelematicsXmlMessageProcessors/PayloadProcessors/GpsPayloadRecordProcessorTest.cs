using System;
using System.Linq;
using CargoWise.Common;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using NUnit.Framework;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	class GpsPayloadRecordProcessorTest : PayloadRecordProcessorTest<GpsPayloadRecordProcessor, GpsPayloadRecord>
	{
		[TestDate(2020, 5, 7)]
		public override void TestAddsRecord()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 12.3, 56.7, 12, 12, 2.32, 12.222, 55.777, 500);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.7, 12.5, 3, 15, 5.71, -55.777, -12.222, 900);
			});
			ErrorReporter.Clear();

			void Test(DateTimeOffset dateTimeOffset, double course, double speed, uint accuracyM, uint numberOfSatellites, double hDop, double latitude, double longitude, int altitude)
			{
				Process(dateTimeOffset, course, speed, accuracyM, numberOfSatellites, hDop, latitude, longitude, altitude);

				var result = device.Locations.Single();
				var expectedTime = dateTimeOffset.UtcDateTime;
				AssertEquals(expectedTime, result.V2_MeasurementTimeUtc);
				AssertEquals((decimal)course, result.V2_CompassHeadingDegrees);
				AssertEquals((decimal)speed, result.V2_Speedkmh);
				AssertEquals(accuracyM, result.V2_AccuracyInMetres);
				AssertEquals(numberOfSatellites, result.V2_SatelliteQuantity);
				AssertEquals((decimal)hDop, result.V2_HDOP);
				AssertEquals(latitude, result.V2_Location.Latitude.Value);
				AssertEquals(longitude, result.V2_Location.Longitude.Value);
				AssertEquals((double)altitude, result.V2_Location.Elevation.Value);
				result.Delete();
			}
		}

		[TestDate(2020, 5, 7, 23, 59, 59)]
		public void TestPayloadRecordFromFutureIgnoredAndReportedAsErrors()
		{
			CombineAssertions(() =>
			{
				AssertPayloadRecordFromFutureAreIgnoredAndReportedAsErrors(new DateTimeOffset(2020, 5, 31, 10, 5, 23, TimeSpan.FromHours(10)));
				AssertPayloadRecordFromFutureAreIgnoredAndReportedAsErrors(new DateTimeOffset(2020, 5, 8, 0, 1, 0, TimeSpan.FromHours(0)));
			});
		}

		void AssertPayloadRecordFromFutureAreIgnoredAndReportedAsErrors(DateTimeOffset deviceDateTime)
		{
			device.V3_HardwareIdentifier = "someiphone";
			device.V3_HardwareKind = GlbDeviceKindCodes.AppleMobile;
			var result = Process(deviceDateTime, 12.3, 56.7, 12, 12, 2.32, 12.222, 55.777, 500);

			AssertEquals("Should not create any locations", 0, device.Locations.Count);
			AssertEquals("Should indicate no locations created", 0, result);

			AssertNotEquals("Should report error", 0, ErrorReporter.TotalErrorCount);
			AssertContains("Should report event times in the future", "V2_MeasurementTimeUtc in the future", ErrorReporter.LastMessageReported);
			AssertContains("Date should be in an unambiguous format", $"V2_MeasurementTimeUtc: {deviceDateTime.ToString("u")}", ErrorReporter.LastMessageReported);
			AssertContains("Should include identifiers about the device", "device.V3_HumanReadableIdentifier: TT00000001, device.V3_HardwareIdentifier: someiphone, device.V3_HardwareKind: IOS", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		[TestDate(2020, 5, 7, 23, 59, 59, 59)]
		public void TestPayloadRecordFromFutureAllowsForSomeClockDrift()
		{
			CombineAssertions(() =>
			{
				AssertPayloadRecordFromFutureAllowsForSomeClockDrift(new DateTimeOffset(2020, 5, 8, 0, 0, 1, TimeSpan.FromHours(10)));
				AssertPayloadRecordFromFutureAllowsForSomeClockDrift(new DateTimeOffset(2020, 5, 8, 0, 0, 30, TimeSpan.FromHours(10)));
			});
		}

		void AssertPayloadRecordFromFutureAllowsForSomeClockDrift(DateTimeOffset deviceDateTime)
		{
			device.V3_HardwareIdentifier = "someiphone";
			device.V3_HardwareKind = GlbDeviceKindCodes.AppleMobile;
			var result = Process(deviceDateTime, 12.3, 56.7, 12, 12, 2.32, 12.222, 55.777, 500);

			AssertNotEquals("Should create locations", 0, device.Locations.Count);
			AssertEquals("Should locations created", 1, result);
			AssertEquals("Should have no error reports", 0, ErrorReporter.TotalErrorCount);
			ErrorReporter.Clear();
		}

		int Process(DateTimeOffset dateTimeOffset, double course, double speed, uint accuracyM, uint numberOfSatellites, double hDop, double latitude, double longitude, int altitude)
		{
			var payloadRecord = new GpsPayloadRecord
			{
				DateTimeOffset = dateTimeOffset,
				Course = course,
				Speed = speed,
				AccuracyM = accuracyM,
				NumberOfSatellites = numberOfSatellites,
				HDop = hDop,
				Latitude = latitude,
				Longitude = longitude,
				Altitude = altitude,
			};

			return processor.Process(Factory, device, payloadRecord);
		}

		public void TestRoadTypeIsMarkedAsPublicWhenProcessGlobalPositionDataOnRoadTypeIsFalse()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 12.3, 56.7, 12, 12, 2.32, 12.222, 55.777, 500);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.7, 12.5, 3, 15, 5.71, -55.777, -12.222, 900);
			});

			void Test(DateTimeOffset dateTimeOffset, double course, double speed, uint accuracyM, uint numberOfSatellites, double hDop, double latitude, double longitude, int altitude)
			{
				// Arrange
				TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
				var payloadRecord = new GpsPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					Course = course,
					Speed = speed,
					AccuracyM = accuracyM,
					NumberOfSatellites = numberOfSatellites,
					HDop = hDop,
					Latitude = latitude,
					Longitude = longitude,
					Altitude = altitude,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.Locations.Single();
				AssertEquals(GlbDeviceLocationRoadTypes.Codes.Public, result.V2_RoadType);
				result.Delete();
			}
		}

		public void RoadTypeIsMarkedAsUnknownWhenProcessGlobalPositionDataOnRoadTypeIsTrue()
		{
			CombineAssertions(() =>
			{
				Test(new DateTimeOffset(2020, 5, 6, 10, 5, 23, TimeSpan.FromHours(10)), 12.3, 56.7, 12, 12, 2.32, 12.222, 55.777, 500);
				Test(new DateTimeOffset(2012, 5, 6, 10, 5, 23, TimeSpan.Zero), 56.7, 12.5, 3, 15, 5.71, -55.777, -12.222, 900);
			});

			void Test(DateTimeOffset dateTimeOffset, double course, double speed, uint accuracyM, uint numberOfSatellites, double hDop, double latitude, double longitude, int altitude)
			{
				// Arrange
				TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
				var payloadRecord = new GpsPayloadRecord
				{
					DateTimeOffset = dateTimeOffset,
					Course = course,
					Speed = speed,
					AccuracyM = accuracyM,
					NumberOfSatellites = numberOfSatellites,
					HDop = hDop,
					Latitude = latitude,
					Longitude = longitude,
					Altitude = altitude,
				};
				var expectedTime = dateTimeOffset.UtcDateTime;

				// Act
				processor.Process(Factory, device, payloadRecord);

				// Assert
				var result = device.Locations.Single();
				AssertEquals(GlbDeviceLocationRoadTypes.Codes.Unknown, result.V2_RoadType);
				result.Delete();
			}
		}
	}
}
