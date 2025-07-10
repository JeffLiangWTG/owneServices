using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.Rim;
using Moq;
using NUnit.Framework;
using WTG.Telematics.Common.Conversion;

namespace Enterprise.Telematics.ServiceTasks.Test.Rim
{
	class DataAccessorTest : TestCaseWithFactory
	{
		protected override void SetUp()
		{
			base.SetUp();
			loggerMock = new Mock<ILogger>();
			dataAccessor = new DataAccessor(loggerMock.Object);
		}

		public void TestWrongParamsCall()
		{
			CombineAssertions(() =>
			{
				var result = AssertExceptionThrown<ArgumentNullException>(() => _ = new DataAccessor(null));
				AssertEquals("logger", result.ParamName);

				result = AssertExceptionThrown<ArgumentNullException>(() => _ = dataAccessor.GetData(null));
				AssertEquals("factory", result.ParamName);
			});
		}

		public void TestNoDevices()
		{
			// Arrange
			_ = CreateDevicesWithLocations(1, 2, 3);

			// Act
			var result = dataAccessor.GetData(Factory).ToArray();

			// Assert
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()), Times.Once);
			loggerMock.Verify(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>(), It.IsAny<Exception>()), Times.Never);
			AssertEquals(0, result.Length);
		}

		public void TestLoadsDataOnlyForRegisteredDevices()
		{
			// Arrange
			var devicesWithLocations = CreateDevicesWithLocations(5, 5, 5, 5);
			var telEdge = Factory.New<TelEdge>();
			telEdge.TE_EntityTableCodeFrom = "RQ";
			telEdge.TE_EntityIdFrom = ZGuid.Empty;
			telEdge.TE_RelationshipType = "DVC";
			telEdge.TE_EntityTableCodeTo = "V3";
			telEdge.TE_EntityIdTo = devicesWithLocations[0].device.PK;
			telEdge.TE_StartTime = new ZDateTimeOffset(2019, 12, 23, 20, 22, 0, TimeSpan.FromHours(11));

			telEdge = Factory.New<TelEdge>();
			telEdge.TE_EntityTableCodeFrom = "RQ";
			telEdge.TE_EntityIdFrom = ZGuid.Empty;
			telEdge.TE_RelationshipType = "RIM";
			telEdge.TE_EntityTableCodeTo = "V3";
			telEdge.TE_EntityIdTo = devicesWithLocations[1].device.PK;
			telEdge.TE_StartTime = new ZDateTimeOffset(2019, 12, 23, 20, 22, 0, TimeSpan.FromHours(11));

			telEdge = Factory.New<TelEdge>();
			telEdge.TE_EntityTableCodeFrom = "RQ";
			telEdge.TE_EntityIdFrom = ZGuid.Empty;
			telEdge.TE_RelationshipType = "RIM";
			telEdge.TE_EntityTableCodeTo = "V3";
			telEdge.TE_EntityIdTo = devicesWithLocations[2].device.PK;
			telEdge.TE_StartTime = new ZDateTimeOffset(2019, 12, 23, 20, 22, 0, TimeSpan.FromHours(11));
			telEdge.TE_EndTime = ZDateTimeOffset.Now;
			Factory.Save();

			// Act
			var result = dataAccessor.GetData(Factory)
				.Single();

			// Assert
			AssertEquals(devicesWithLocations[1].device.V3_HardwareIdentifier, result.DeviceId);
			AssertContainsExactElementsInAnyOrder(devicesWithLocations[1].locations, result.Locations);
		}

		public void TestLoadsDataOnlyForEmbeddedDevices()
		{
			// Arrange
			var devicesWithLocations = CreateDevicesWithLocations(5, 5, 5, 5, 5);
			_ = devicesWithLocations
				.Select(tuple =>
				{
					var telEdge = Factory.New<TelEdge>();
					telEdge.TE_EntityTableCodeFrom = "RQ";
					telEdge.TE_EntityIdFrom = ZGuid.Empty;
					telEdge.TE_RelationshipType = "RIM";
					telEdge.TE_EntityTableCodeTo = "V3";
					telEdge.TE_EntityIdTo = tuple.device.PK;
					telEdge.TE_StartTime = new ZDateTimeOffset(2019, 12, 23, 20, 22, 0, TimeSpan.FromHours(11));
					return telEdge;
				})
				.ToArray();

			devicesWithLocations[0].device.V3_HardwareKind = GlbDeviceKindCodes.Android;
			devicesWithLocations[1].device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
			devicesWithLocations[2].device.V3_HardwareKind = GlbDeviceKindCodes.AppleMobile;
			devicesWithLocations[3].device.V3_HardwareKind = GlbDeviceKindCodes.WindowsMobileLegacy;
			devicesWithLocations[4].device.V3_HardwareKind = GlbDeviceKindCodes.Unknown;

			Factory.Save();

			// Act
			var result = dataAccessor.GetData(Factory)
				.Single();

			// Assert
			AssertEquals(devicesWithLocations[1].device.V3_HardwareIdentifier, result.DeviceId);
			AssertContainsExactElementsInAnyOrder(devicesWithLocations[1].locations, result.Locations);
		}

		public void TestLoadsDataOnlyForUnsentLocations()
		{
			// Arrange
			var devicesWithLocations = CreateDevicesWithLocations(15, 15, 15);
			_ = devicesWithLocations
				.Select(tuple =>
				{
					var telEdge = Factory.New<TelEdge>();
					telEdge.TE_EntityTableCodeFrom = "RQ";
					telEdge.TE_EntityIdFrom = ZGuid.Empty;
					telEdge.TE_RelationshipType = "RIM";
					telEdge.TE_EntityTableCodeTo = "V3";
					telEdge.TE_EntityIdTo = tuple.device.PK;
					telEdge.TE_StartTime = new ZDateTimeOffset(2019, 12, 23, 20, 22, 0, TimeSpan.FromHours(11));
					return telEdge;
				})
				.ToArray();

			for (var i = 0; i < 5; i++)
			{
				devicesWithLocations[0].locations[i].V2_RimReported = true;
				devicesWithLocations[1].locations[i + 5].V2_RimReported = true;
				devicesWithLocations[2].locations[i + 10].V2_RimReported = true;
			}

			Factory.Save();

			// Act
			var result = dataAccessor.GetData(Factory)
				.ToDictionary(data => data.DeviceId, data => data.Locations.ToArray());

			// Assert
			AssertContainsExactElementsInAnyOrder(
				devicesWithLocations[0].locations.Skip(5),
				result[devicesWithLocations[0].device.V3_HardwareIdentifier]);
			AssertContainsExactElementsInAnyOrder(
				devicesWithLocations[1].locations.Take(5).Concat(devicesWithLocations[1].locations.Skip(10)),
				result[devicesWithLocations[1].device.V3_HardwareIdentifier]);
			AssertContainsExactElementsInAnyOrder(
				devicesWithLocations[2].locations.Take(10),
				result[devicesWithLocations[2].device.V3_HardwareIdentifier]);
		}

		[ExpectNoExceptions]
		public void TestDoesNotLoadLocationsWithoutForcedEnumerating()
		{
			// Arrange
			var devicesWithLocations = CreateDevicesWithLocations(15);
			_ = devicesWithLocations
				.Select(tuple =>
				{
					var telEdge = Factory.New<TelEdge>();
					telEdge.TE_EntityTableCodeFrom = "RQ";
					telEdge.TE_EntityIdFrom = ZGuid.Empty;
					telEdge.TE_RelationshipType = "RIM";
					telEdge.TE_EntityTableCodeTo = "V3";
					telEdge.TE_EntityIdTo = tuple.device.PK;
					telEdge.TE_StartTime = new ZDateTimeOffset(2019, 12, 23, 20, 22, 0, TimeSpan.FromHours(11));
					return telEdge;
				})
				.ToArray();
			Factory.Save();

			var factoryMock = new Mock<IFactory>(MockBehavior.Strict);
			factoryMock
				.Setup(factory => factory.Load<TelEdge>(It.IsAny<ZQuery>()))
				.Returns<ZQuery>(Factory.Load<TelEdge>);
			factoryMock
				.Setup(factory => factory.Load<GlbDevice>(It.IsAny<ZQuery>()))
				.Returns<ZQuery>(Factory.Load<GlbDevice>);

			// Act
			_ = dataAccessor.GetData(factoryMock.Object);

			// Assert
			factoryMock.Verify(factory => factory.Load<GlbDeviceLocation>(It.IsAny<ZQuery>()), Times.Never);
		}

		(GlbDevice device, GlbDeviceLocation[] locations)[] CreateDevicesWithLocations(params int[] locationAmountPerDevice)
		{
			return locationAmountPerDevice
				.Select((count, i) =>
				{
					var device = Factory.New<GlbDevice>();
					device.V3_HumanReadableIdentifier = $"Device{i:D5}";
					device.V3_IsActive = true;
					device.V3_MobileServicesIdentifier = BitConverter.GetBytes(10 + i);
					device.V3_HardwareIdentifier = BinaryDataConverter.ByteArrayToHexString(device.V3_MobileServicesIdentifier);
					device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
					device.V3_Model = "Model";

					var locations = Enumerable
						.Range(1, count)
						.Select(i1 =>
						{
							var location = Factory.New<GlbDeviceLocation>();
							location.V2_V3_Device = device.PK;
							location.V2_RimReported = false;
							location.V2_MeasurementTimeUtc = ZDateTime.Now;
							return location;
						});
					return (device, locations.ToArray());
				})
				.ToArray();
		}

		DataAccessor dataAccessor;
		Mock<ILogger> loggerMock;
	}
}
