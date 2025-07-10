using System;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.MobileServices.Common.Messages.EHub;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.Business.Registry;
using Enterprise.Telematics.ServiceTasks.MessageProcessing;
using Moq;
using NUnit.Framework;

namespace Enterprise.Telematics.ServiceTasks.Test.MessageProcessing
{
	class M2CDeviceLocationDataNotificationProcessorTests : TestCaseWithFactory
	{
		public void TestCreatesLocationReferencingAssignedOwnerWhenStartAndEndTimesAreTheSame()
		{
			processor.Process(Factory, string.Empty, CreateTypicalMessage(deviceIdentifier));
			Factory.Save();

			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals((short)1, location.V2_AccuracyInMetres);
			AssertEquals(-22.0, location.V2_Location.Elevation);
			AssertEquals(203.0m, location.V2_CompassHeadingDegrees);
			AssertEquals(-33.914816, location.V2_Location.Latitude);
			AssertEquals(151.195435, location.V2_Location.Longitude);
			AssertEquals(0.5m, location.V2_Speedkmh);
			AssertEquals(new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc), location.V2_MeasurementTimeUtc);
			AssertEquals(device.PK, location.V2_V3_Device);
		}

		[ExpectNoExceptions]
		public void TestProcessLogsWarningIfReceivedDataFromUnknownDevice()
		{
			// Arrange
			var identifier = new byte[] { 1, 2, 3, 4, 5, 6 };
			var message = CreateTypicalMessage(identifier);
			loggerMock.Setup(logger => logger.Log(LogType.Warning, "Received location data for unknown device [010203040506]. Locations record(s) skipped: 1."));

			// Act
			processor.Process(Factory, string.Empty, message);

			// Assert
			loggerMock.VerifyAll();
		}

		public void TestProcessDoesNotAddLocationsIfReceivedDataFromUnknownDevice()
		{
			// Arrange
			var message = CreateTypicalMessage(new byte[] { 1, 2, 3, 4, 5, 6 });
			processor.Process(Factory, string.Empty, message);
			Factory.Save();

			// Act
			var locations = Factory.Load<GlbDeviceLocation>(new ZQuery());

			// Assert
			AssertEquals(0, locations.Length);
		}

		public void TestWrongConstructorParamsCall()
		{
			// Arrange
			// Act
			// Assert
			AssertExceptionThrown<ArgumentNullException>(() => new M2CDeviceLocationDataNotificationProcessor(null));
		}

		public void TestAllLocationsFromMessageAreSaved()
		{
			// Arrange
			var notificationMessage = new M2CDeviceLocationDataNotificationMessage { device_identifier = deviceIdentifier };
			notificationMessage.device_locations.AddRange(Enumerable.Range(0, 543).Select(i => CreateDeviceLocation()));
			var message = new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = notificationMessage.Serialize()
			};

			// Act
			processor.Process(Factory, string.Empty, message);
			Factory.Save();

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals(notificationMessage.device_locations.Count, device.Locations.Count);
		}

		public void TestRoadTypeIsMarkedAsPublicWhenProcessGlobalPositionDataOnRoadTypeIsFalse()
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);
			var notificationMessage = new M2CDeviceLocationDataNotificationMessage { device_identifier = deviceIdentifier };
			notificationMessage.device_locations.Add(CreateDeviceLocation());
			var message = new EHubMessageContainer { message_type = EHubMessageType.M2CDeviceLocationDataNotification, message_data = notificationMessage.Serialize() };

			// Act
			processor.Process(Factory, string.Empty, message);
			Factory.Save();

			// Assert
			var result = Factory.Load<GlbDevice>(devicePK)
				.Locations
				.Single();
			AssertEquals(GlbDeviceLocationRoadTypes.Codes.Public, result.V2_RoadType);

			// Cleanup
			result.Delete();
		}

		public void RoadTypeIsMarkedAsUnknownWhenProcessGlobalPositionDataOnRoadTypeIsTrue()
		{
			// Arrange
			TelematicsConfigurationRegistry.Instance.ProcessGlobalPositionDataOnRoadType.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var notificationMessage = new M2CDeviceLocationDataNotificationMessage { device_identifier = deviceIdentifier };
			notificationMessage.device_locations.Add(CreateDeviceLocation());
			var message = new EHubMessageContainer { message_type = EHubMessageType.M2CDeviceLocationDataNotification, message_data = notificationMessage.Serialize() };

			// Act
			processor.Process(Factory, string.Empty, message);
			Factory.Save();

			// Assert
			var result = Factory.Load<GlbDevice>(devicePK)
				.Locations
				.Single();
			AssertEquals(GlbDeviceLocationRoadTypes.Codes.Unknown, result.V2_RoadType);

			// Cleanup
			result.Delete();
		}

		public void TestLocationsFromMessageAreSavedUpToTheBrokenOne()
		{
			// Arrange
			var notificationMessage = new M2CDeviceLocationDataNotificationMessage { device_identifier = deviceIdentifier };
			notificationMessage.device_locations.AddRange(Enumerable.Range(0, 123).Select(i => CreateDeviceLocation()));
			notificationMessage.device_locations[100].compass_heading_degrees_decimal = "-1.0";
			var message = new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = notificationMessage.Serialize()
			};

			// Act
			AssertExceptionThrown<ZSaveException>(() => processor.Process(Factory.CreateNewFactory(), string.Empty, message));

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals(100, device.Locations.Count);
		}

		public void TestCreatesLocationReferencingAssignedOwner()
		{
			processor.Process(Factory, string.Empty, CreateMessage(deviceIdentifier));
			Factory.Save();

			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals((short)15, location.V2_AccuracyInMetres);
			AssertEquals(17d, location.V2_Location.Elevation);
			AssertEquals(160m, location.V2_CompassHeadingDegrees);
			AssertEquals(-33.931189d, location.V2_Location.Latitude);
			AssertEquals(151.175125d, location.V2_Location.Longitude);
			AssertEquals(0m, location.V2_Speedkmh);
			AssertEquals(new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc), location.V2_MeasurementTimeUtc);
			AssertEquals(device.PK, location.V2_V3_Device);
		}

		public void TestDefaultZerosForEmptyOrMissingFields()
		{
			var message = new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = new M2CDeviceLocationDataNotificationMessage
				{
					device_identifier = deviceIdentifier,
					device_locations =
						{
							new M2CDeviceLocationDataNotificationMessage.DeviceLocation()
						},
					device_key = new DeviceKey
					{
						kind = DeviceKind.WiseTechVehicularPlatform,
						identifier = "my-ivu",
					},
				}.Serialize()
			};

			processor.Process(Factory, string.Empty, message);
			Factory.Save();

			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals((short)0, location.V2_AccuracyInMetres);
			AssertEquals(0d, location.V2_Location.Elevation);
			AssertEquals(0m, location.V2_CompassHeadingDegrees);
			AssertEquals(0d, location.V2_Location.Latitude);
			AssertEquals(0d, location.V2_Location.Longitude);
			AssertEquals(0m, location.V2_Speedkmh);
			AssertEquals(new DateTime(1970, 01, 01, 00, 00, 00, DateTimeKind.Utc), location.V2_MeasurementTimeUtc);
			AssertEquals(device.PK, location.V2_V3_Device);
		}

		[ExpectNoExceptions]
		public void TestCompassHeadingIsRoundedUpTo360()
		{
			// Arrange
			var message = CreateMessageWithCompassHeading(deviceIdentifier, "359.97");

			// Act
			// Assert
			processor.Process(Factory, string.Empty, message);
		}

		public void TestUpdatesDeviceKey()
		{
			processor.Process(Factory, string.Empty, CreateMessage(deviceIdentifier));
			Factory.Save();

			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals(GlbDeviceKindCodes.WTGEmbedded, device.V3_HardwareKind);
			AssertEquals("my-ivu", device.V3_HardwareIdentifier);
		}

		public void TestReportsErrorWhenDateIsFarInFuture()
		{
			TestReportsErrorWhenDateIsInFutureCore(Env.Time.CurrentLocalDateTime.AddYears(1));
		}

		public void TestReportsErrorWhenDateIsThreeHoursInTheFuture()
		{
			TestReportsErrorWhenDateIsInFutureCore(Env.Time.CurrentLocalDateTime.AddHours(3));
		}

		void TestReportsErrorWhenDateIsInFutureCore(DateTime timeFrom)
		{
			processor.Process(Factory, string.Empty, CreateMessageFromFuture(deviceIdentifier, timeFrom));
			Factory.Save();

			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should not have a location, as date is from the future", 0, device.Locations.Count);

			AssertContains("Should report event times in the future", "time_from_utc in the future", ErrorReporter.LastMessageReported);
			AssertContains("Date should be in an unambiguous format", $"time_from_utc: {timeFrom:yyyy-MM-dd}", ErrorReporter.LastMessageReported);
			AssertContains("Should include identifiers about the device", "device.V3_HumanReadableIdentifier: TT00000001, device.V3_HardwareIdentifier: my-ivu, device.V3_HardwareKind: EMB", ErrorReporter.LastMessageReported);

			ErrorReporter.Clear();
		}

		[TestDate(2023, 5, 11, 23, 15, 0)]
		[TestUtcOffset(0, 0, 0)]
		public void TestDoesNotReportErrorWhenDateIsInVeryNearFuture()
		{
			var timeFrom = Env.Time.CurrentLocalDateTime.AddHours(1);
			processor.Process(Factory, string.Empty, CreateMessageFromFuture(deviceIdentifier, timeFrom));
			Factory.Save();

			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location, as date is valid", 1, device.Locations.Count);

			AssertEquals("Should not report event times in the near future - even if from early the next day", 0, ErrorReporter.LastExceptionsReported().Count);

			ErrorReporter.Clear();
		}

		public void TestLocationWithTooHighAccuracy()
		{
			var message = CreateMessageWithAccuracy(deviceIdentifier, short.MaxValue + 1);
			loggerMock.Setup(logger => logger.Log(LogType.Warning, $"Location has accuracy_in_metres too high: value is {short.MaxValue + 1}, maximum is 32767. Value has been set to maximum."));

			AssertNoExceptionThrown("Should not encounter exception", () =>
			{
				processor.Process(Factory, string.Empty, message);
			});

			loggerMock.VerifyAll();
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals("Should cut off V2_AccuracyInMetres to maximum short value", short.MaxValue, location.V2_AccuracyInMetres);
		}

		public void TestLocationWithNotTooHighAccuracy()
		{
			var message = CreateMessageWithAccuracy(deviceIdentifier, short.MaxValue - 1);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Never());
			processor.Process(Factory, string.Empty, message);

			loggerMock.VerifyAll();
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals("Should set V2_AccuracyInMetres to value supplied", (short)(short.MaxValue - 1), location.V2_AccuracyInMetres);
		}

		public void TestLocationWithBelowMinValueAccuracy()
		{
			var message = CreateMessageWithAccuracy(deviceIdentifier, short.MinValue - 1);
			loggerMock.Setup(logger => logger.Log(LogType.Warning, $"Location has accuracy_in_metres too low: value is {short.MinValue - 1}, minimum is 0. Value has been set to minimum."));

			AssertNoExceptionThrown("Should not encounter exception, either due to overflow or database validation", () =>
			{
				processor.Process(Factory, string.Empty, message);
			});

			loggerMock.VerifyAll();
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals("Should set V2_AccuracyInMetres to zero", (short)0, location.V2_AccuracyInMetres);
		}

		public void TestLocationWithNegativeAccuracy()
		{
			var message = CreateMessageWithAccuracy(deviceIdentifier, -1);
			loggerMock.Setup(logger => logger.Log(LogType.Warning, $"Location has accuracy_in_metres too low: value is -1, minimum is 0. Value has been set to minimum."));

			AssertNoExceptionThrown("Should not encounter exception due to database validation", () =>
			{
				processor.Process(Factory, string.Empty, message);
			});

			loggerMock.VerifyAll();
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have a location", 1, device.Locations.Count);

			var location = device.Locations[0];
			AssertEquals("Should set V2_AccuracyInMetres to zero", (short)0, location.V2_AccuracyInMetres);
		}

		#region Implementation

		byte[] deviceIdentifier;
		ZGuid devicePK;
		ZGuid truck1PK;
		ZGuid truck2PK;
		ZGuid truck3PK;
		M2CDeviceLocationDataNotificationProcessor processor;
		Mock<ILogger> loggerMock;
		GlbDevice device;

		static EHubMessageContainer CreateMessage(byte[] deviceIdentifier)
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = new M2CDeviceLocationDataNotificationMessage
				{
					device_identifier = deviceIdentifier,
					device_locations =
					{
						new M2CDeviceLocationDataNotificationMessage.DeviceLocation
						{
							accuracy_in_metres = 15,
							altitude_in_metres_decimal = "17",
							compass_heading_degrees_decimal = "160",
							latitude_decimal = "-33.931189",
							longitude_decimal = "151.175125",
							sample_quantity = 2,
							speed_kmph_decimal = "0",
							time_from_utc = new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc).AsPosixTime(),
							time_to_utc = new DateTime(2015, 07, 17, 11, 34, 12, DateTimeKind.Utc).AsPosixTime()
						}
					},
					device_key = new DeviceKey
					{
						kind = DeviceKind.WiseTechVehicularPlatform,
						identifier = "my-ivu",
					},
				}.Serialize()
			};
		}

		static EHubMessageContainer CreateMessageWithCompassHeading(byte[] deviceIdentifier, string compassHeadingDegrees)
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = new M2CDeviceLocationDataNotificationMessage
				{
					device_identifier = deviceIdentifier,
					device_locations =
					{
						new M2CDeviceLocationDataNotificationMessage.DeviceLocation
						{
							accuracy_in_metres = 15,
							altitude_in_metres_decimal = "17",
							compass_heading_degrees_decimal = compassHeadingDegrees,
							latitude_decimal = "-33.931189",
							longitude_decimal = "151.175125",
							sample_quantity = 2,
							speed_kmph_decimal = "0",
							time_from_utc = new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc).AsPosixTime(),
							time_to_utc = new DateTime(2015, 07, 17, 11, 34, 12, DateTimeKind.Utc).AsPosixTime()
						}
					}
				}.Serialize()
			};
		}

		static EHubMessageContainer CreateTypicalMessage(byte[] deviceIdentifier)
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = new M2CDeviceLocationDataNotificationMessage
				{
					device_identifier = deviceIdentifier,
					device_locations =
					{
						CreateDeviceLocation()
					}
				}.Serialize()
			};
		}

		static EHubMessageContainer CreateMessageFromFuture(byte[] deviceIdentifier, DateTime timeFrom)
		{
			return new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = new M2CDeviceLocationDataNotificationMessage
				{
					device_identifier = deviceIdentifier,
					device_locations =
					{
						new M2CDeviceLocationDataNotificationMessage.DeviceLocation
						{
							accuracy_in_metres = 15,
							altitude_in_metres_decimal = "17",
							compass_heading_degrees_decimal = "160",
							latitude_decimal = "-33.931189",
							longitude_decimal = "151.175125",
							sample_quantity = 2,
							speed_kmph_decimal = "0",
							time_from_utc = timeFrom.AsPosixTime(),
							time_to_utc = timeFrom.AddMinutes(1).AsPosixTime()
						}
					},
					device_key = new DeviceKey
					{
						kind = DeviceKind.WiseTechVehicularPlatform,
						identifier = "my-ivu",
					},
				}.Serialize()
			};
		}

		static EHubMessageContainer CreateMessageWithAccuracy(byte[] deviceIdentifier, int accuracyInMetres)
		{
			var deviceLocation = CreateDeviceLocation();
			deviceLocation.accuracy_in_metres = accuracyInMetres;
			return CreateMessageGivenLocation(deviceIdentifier, deviceLocation);
		}

		static EHubMessageContainer CreateMessageGivenLocation(byte[] deviceIdentifier, M2CDeviceLocationDataNotificationMessage.DeviceLocation deviceLocation)
		{
			var notificationMessage = new M2CDeviceLocationDataNotificationMessage { device_identifier = deviceIdentifier };

			notificationMessage.device_locations.Add(deviceLocation);

			var message = new EHubMessageContainer
			{
				message_type = EHubMessageType.M2CDeviceLocationDataNotification,
				message_data = notificationMessage.Serialize()
			};
			return message;
		}

		static M2CDeviceLocationDataNotificationMessage.DeviceLocation CreateDeviceLocation()
		{
			return new M2CDeviceLocationDataNotificationMessage.DeviceLocation
			{
				accuracy_in_metres = 1,
				altitude_in_metres_decimal = "-22.0",
				compass_heading_degrees_decimal = "203.0",
				latitude_decimal = "-33.914816",
				longitude_decimal = "151.195435",
				sample_quantity = 1,
				speed_kmph_decimal = "0.5",
				time_from_utc = new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc).AsPosixTime(),
				time_to_utc = new DateTime(2015, 07, 17, 11, 34, 00, DateTimeKind.Utc).AsPosixTime()
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			deviceIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF }; // No significance

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_MobileServicesIdentifier = deviceIdentifier;
			device.V3_Model = "GLaDOS v3.1";
			devicePK = device.PK;

			var truck = Factory.New<RefEquipment>();
			truck.RQ_Registration = "TEST000001";
			truck.RQ_ShortCode = "TEST1";
			truck1PK = truck.PK;

			var assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = new DateTime(2015, 07, 17, 11, 00, 00, DateTimeKind.Utc);
			assignment.V7_EndTimeUtc = new DateTime(2015, 07, 17, 13, 00, 00, DateTimeKind.Utc);
			assignment.V7_V3_Device = devicePK;
			assignment.V7_ParentID = truck1PK;
			assignment.V7_ParentTableCode = "RQ";

			truck = Factory.New<RefEquipment>();
			truck.RQ_Registration = "TEST000002";
			truck.RQ_ShortCode = "TEST2";
			truck2PK = truck.PK;

			assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = new DateTime(2015, 07, 17, 13, 00, 00, DateTimeKind.Utc);
			assignment.V7_EndTimeUtc = new DateTime(2015, 07, 18, 00, 00, 00, DateTimeKind.Utc);
			assignment.V7_V3_Device = devicePK;
			assignment.V7_ParentID = truck2PK;
			assignment.V7_ParentTableCode = "RQ";

			truck = Factory.New<RefEquipment>();
			truck.RQ_Registration = "TEST000003";
			truck.RQ_ShortCode = "TEST3";
			truck3PK = truck.PK;

			assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = new DateTime(2015, 07, 19, 00, 00, 00, DateTimeKind.Utc);
			assignment.V7_V3_Device = devicePK;
			assignment.V7_ParentID = truck3PK;
			assignment.V7_ParentTableCode = "RQ";

			Factory.Save();

			loggerMock = new Mock<ILogger>();
			processor = new M2CDeviceLocationDataNotificationProcessor(loggerMock.Object);
		}

		protected override void TearDown()
		{
			processor = null;
			deviceIdentifier = null;

			base.TearDown();
		}

		#endregion
	}
}
