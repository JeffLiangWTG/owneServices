using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.Telematics.Common.Conversion;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;
using WTG.Telematics.Interfaces.Packets.V2;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class DeviceConnectionReportPayloadRecordProcessorTest : TestCaseWithFactory
	{
		public void TestB1232Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var aTrailer3 = CreateVehicle("03", "3", "A Trailer");
			var bTrailer2 = CreateVehicle("04", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					aTrailer3,
					bTrailer2,
				},
				"B1232");
		}

		public void TestA122Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var bTrailer2 = CreateVehicle("04", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					bTrailer2,
				},
				"A122");
		}

		public void TestB12333Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var aTrailer3 = CreateVehicle("03", "3", "A Trailer");
			var secondATrailer3 = CreateVehicle("09", "3", "A Trailer");
			var bTrailer3 = CreateVehicle("08", "3", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(secondATrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					aTrailer3,
					secondATrailer3,
					bTrailer3
				},
				"B12333");
		}

		public void TestB12332Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var aTrailer3 = CreateVehicle("03", "3", "A Trailer");
			var secondATrailer3 = CreateVehicle("09", "3", "A Trailer");
			var bTrailer2 = CreateVehicle("08", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(secondATrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					aTrailer3,
					secondATrailer3,
					bTrailer2
				},
				"B12332");
		}

		public void TestB12222Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var aTrailer2 = CreateVehicle("03", "2", "A Trailer");
			var secondATrailer2 = CreateVehicle("09", "2", "A Trailer");
			var bTrailer2 = CreateVehicle("08", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(secondATrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					aTrailer2,
					secondATrailer2,
					bTrailer2
				},
				"B12222");
		}

		public void TestL11Configuration()
		{
			var lightVehicle11 = CreateVehicle(deviceIdentifier, "11", "Light Vehicle");
			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(lightVehicle11.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				lightVehicle11,
				new List<TelSubEquipment>()
				{
					lightVehicle11,
				},
				"L11");
		}

		public void TestB1233T2B33Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var aTrailer31 = CreateVehicle("07", "3", "a trailer");
			var bTrailer31 = CreateVehicle("04", "3", "b trailer");
			var dolly21 = CreateVehicle("06", "2", "dolly");
			var bTrailer32 = CreateVehicle("03", "3", "b trailer");
			var aTrailer32 = CreateVehicle("05", "3", "a trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer31.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer31.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(dolly21.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer32.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer32.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					aTrailer31,
					bTrailer31,
					dolly21,
					aTrailer32,
					bTrailer32,
				},
				"B1233T2B33");
		}

		public void TestA123T33T33Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var bTrailer31 = CreateVehicle("07", "3", "b trailer");
			var dolly31 = CreateVehicle("06", "3", "dolly");
			var bTrailer32 = CreateVehicle("05", "3", "B trailer");
			var dolly32 = CreateVehicle("04", "3", "DOLLY");
			var bTrailer33 = CreateVehicle("03", "3", "B trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer31.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(dolly31.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer32.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(dolly32.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer33.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					bTrailer31,
					dolly31,
					bTrailer32,
					dolly32,
					bTrailer33,
				},
				"A123T33T33");
		}

		public void TestA123T33Configuration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var bTrailer31 = CreateVehicle("07", "3", "b trailer");
			var dolly31 = CreateVehicle("06", "3", "dolly");
			var bTrailer32 = CreateVehicle("05", "3", "B trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer31.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(dolly31.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer32.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(
				primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					bTrailer31,
					dolly31,
					bTrailer32,
				},
				"A123T33");
		}

		public void TestPrimeMoverConfiguration()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var someAxle = CreateSubEquipment("05", "<Axle />", "A");
			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(primeMover12, new List<TelSubEquipment>() { primeMover12 }, "A12");
		}

		public void TestInvariantCase()
		{
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "prime mover");
			var aTrailer3 = CreateVehicle("03", "3", "a trailer");
			var bTrailer2 = CreateVehicle("04", "2", "b trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(primeMover12,
				new List<TelSubEquipment>()
				{
					primeMover12,
					aTrailer3,
					bTrailer2,
				},
				"B1232");
		}

		public void TestRigidTruck()
		{
			var rigidTruck11 = CreateVehicle(deviceIdentifier, "11", "Rigid Truck");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(rigidTruck11.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(rigidTruck11,
				new List<TelSubEquipment>()
				{
					rigidTruck11,
				},
				"R11");
		}

		public void TestInvariantCaseRigidTruck()
		{
			var rigidTruck11 = CreateVehicle(deviceIdentifier, "11", "rigid truck");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(rigidTruck11.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(rigidTruck11,
				new List<TelSubEquipment>()
				{
					rigidTruck11,
				},
				"R11");
		}

		public void TestMultipleEquipmentsPerVehicle()
		{
			var firstPrimeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var secondPrimeMover12 = CreateVehicle(deviceIdentifier, "13", "Prime Mover");
			var aTrailer3 = CreateVehicle("03", "3", "A Trailer");
			var bTrailer2 = CreateVehicle("04", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(firstPrimeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(secondPrimeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-3), messageTime.AddDays(-2));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(firstPrimeMover12,
				new List<TelSubEquipment>()
				{
					firstPrimeMover12,
					aTrailer3,
					bTrailer2,
				},
				"B1232");
		}

		public void TestMultipleEquipmentsPerVehicleMessageOnClosedEdge()
		{
			var firstPrimeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var secondPrimeMover12 = CreateVehicle(deviceIdentifier, "13", "Prime Mover");
			var aTrailer3 = CreateVehicle("03", "3", "A Trailer");
			var bTrailer2 = CreateVehicle("04", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(firstPrimeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(4));
			CreateEdge(secondPrimeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2), messageTime.AddDays(2));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			ValidConfiguration(secondPrimeMover12,
				new List<TelSubEquipment>()
				{
					secondPrimeMover12,
					aTrailer3,
					bTrailer2,
				},
				"B1332");
		}

		public void TestMultipleEquipmentWithOpenEdges()
		{
			// Arrange
			loggerMock.Setup(logger => logger.Log(LogType.Warning, $"Could not retrieve equipment from db for attached devices [{deviceIdentifier}, 03, 04]"));

			var firstPrimeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var secondPrimeMover12 = CreateVehicle(deviceIdentifier, "13", "Prime Mover");
			var aTrailer3 = CreateVehicle("03", "3", "A Trailer");
			var bTrailer2 = CreateVehicle("04", "2", "B Trailer");

			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(firstPrimeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(secondPrimeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-3));
			CreateEdge(aTrailer3.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));

			var connectedVehicleIds = new List<TelSubEquipment>() { firstPrimeMover12, aTrailer3, bTrailer2, }
				.Select(vehicle => vehicle.TSE_Id.ToString()).ToList();
			var message = CreateMessage(deviceIdentifier, connectedVehicleIds, messageTime);

			// Act
			processor.Process(Factory, device, message);

			// Assert
			loggerMock.Verify(logger => logger.Log(LogType.Warning, $"Could not retrieve equipment from db for attached devices [{deviceIdentifier}, 03, 04]"), Times.Once);
			var combinationReports = Factory.Load<GlbDeviceCombinationReport>(new ZQuery(GlbDeviceCombinationReportSchema.GDC_V3_Device, device.PK));
			AssertEquals(0, combinationReports.Length);
		}

		void ValidConfiguration(TelSubEquipment vehicleId, IList<TelSubEquipment> connectedVehicles, string expectedCode)
		{
			// Arrange
			var expectedTime = new ZDateTime(messageTime.UtcDateTime);
			var connectedVehicleIds = connectedVehicles
				.Select(vehicle => vehicle.TSE_Id.ToString())
				.ToList();
			var message = CreateMessage(vehicleId.TSE_Id, connectedVehicleIds, messageTime);

			// Act
			processor.Process(Factory, device, message);

			// Assert
			var combinationReport = Factory.LoadTop1<GlbDeviceCombinationReport>(new ZQuery(GlbDeviceCombinationReportSchema.GDC_V3_Device, device.PK));
			AssertEquals(expectedCode, combinationReport.GDC_CombinationCode);
			AssertEquals(expectedTime, combinationReport.GDC_MeasurementTimeUtc);
		}

		public void TestInvalidConfiguration()
		{
			// Arrange
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");
			var bTrailer2 = CreateVehicle("08", "2", "B Trailer");
			var someAxle = CreateSubEquipment("05", "<Axle />", "A");

			CreateEdge(primeMover12.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-2));
			CreateEdge(bTrailer2.PK.ToGuid(), someAxle.PK.ToGuid(), messageTime.AddDays(-3));

			var hardwareIds = new List<string>()
			{
				bTrailer2.TSE_Id,
				primeMover12.TSE_Id
			};
			var message = CreateMessage(
				primeMover12.TSE_Id,
				hardwareIds,
				messageTime);
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			// Act
			processor.Process(Factory, device, message);

			// Assert
			loggerMock.Verify(logger => logger.Log(
				LogType.Error,
				$"Device connection report [b trailer: {hardwareIds[0]}, prime mover: {hardwareIds[1]}] could not map to a valid combination code"),
				Times.Once);

			var combinationReports = Factory.Load<GlbDeviceCombinationReport>(new ZQuery(GlbDeviceCombinationReportSchema.GDC_V3_Device, device.PK));
			AssertEquals(0, combinationReports.Length);
		}

		public void TestUnknownVehicleInReport()
		{
			// Arrange
			var primeMover12 = CreateVehicle(deviceIdentifier, "12", "Prime Mover");

			var hardwareIds = new List<string>()
			{
				primeMover12.TSE_Id,
				"040004",
			};
			var message = CreateMessage(
				primeMover12.TSE_Id,
				hardwareIds,
				messageTime);
			loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));

			// Act
			processor.Process(Factory, device, message);

			// Assert
			loggerMock.Verify(logger => logger.Log(
					LogType.Warning,
					$"Could not retrieve equipment from db for attached devices [{hardwareIds[0]}, {hardwareIds[1]}]"),
				Times.Once);

			var combinationReports = Factory.Load<GlbDeviceCombinationReport>(new ZQuery(GlbDeviceCombinationReportSchema.GDC_V3_Device, device.PK));
			AssertEquals(0, combinationReports.Length);
		}

		protected override void SetUp()
		{
			base.SetUp();
			deviceIdentifier = "010407FF";

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TT00000001";
			device.V3_IsActive = true;
			device.V3_MobileServicesIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF };
			device.V3_Model = "GLaDOS v3.1";

			Factory.Save();

			messageTime = DateTimeOffset.Now.AddDays(-10);
			loggerMock = new Mock<ILogger>();
			processor = new DeviceConnectionReportPayloadRecordProcessor(loggerMock.Object);
		}

		TelSubEquipment CreateVehicle(string id, string axleConfig, string vehicleType)
		{
			return CreateSubEquipment(id, string.Format(
				CultureInfo.InvariantCulture,
				new XElement(
					"Configuration",
					new XElement("vehicleType", vehicleType),
					new XElement("axleConfig", axleConfig)).ToString()),
				"RQ");
		}

		TelSubEquipment CreateSubEquipment(string id, string config, string type)
		{
			var equipment = Factory.New<TelSubEquipment>();
			equipment.TSE_Id = id;
			equipment.TSE_Type = type;
			equipment.TSE_Configuration = config;
			Factory.Save();
			return equipment;
		}

		TelEdge CreateEdge(Guid edgeFrom, Guid edgeTo, DateTimeOffset startTime, DateTimeOffset? endTime = null)
		{
			var edge = Factory.New<TelEdge>();
			edge.TE_EntityIdFrom = edgeFrom;
			edge.TE_EntityIdTo = edgeTo;
			edge.TE_StartTime = startTime;
			edge.TE_RelationshipType = "HW";
			edge.TE_EntityTableCodeFrom = "TSE";
			edge.TE_EntityTableCodeTo = "TSE";
			if (endTime != null)
			{
				edge.TE_EndTime = (DateTimeOffset)endTime;
			}

			Factory.Save();

			return edge;
		}

		static DeviceHeartbeatPayloadRecord CreateMessage(string deviceIdentifier, IList<string> connections, DateTimeOffset dateTime)
		{
			return new DeviceHeartbeatPayloadRecord
			{
				DateTimeOffset = dateTime,
				DeviceHeartbeats = connections.Select(connection => new DeviceHeartbeatData
				{
					HeartbeatTime = dateTime,
					DeviceId = new DeviceIdStub(connection),
				})
			};
		}

		DateTimeOffset messageTime;
		Mock<ILogger> loggerMock;
		DeviceConnectionReportPayloadRecordProcessor processor;
		string deviceIdentifier;
		GlbDevice device;

		class DeviceIdStub : IDeviceId
		{
			public DeviceIdStub(string hexString)
			{
				DeviceHex = hexString;
			}

			public byte[] ToBytes()
			{
				return BinaryDataConverter.HexStringToByteArray(DeviceHex);
			}

			public string ToHexString()
			{
				return DeviceHex;
			}

			string DeviceHex { get; }
		}
	}
}
