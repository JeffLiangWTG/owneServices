using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Moq;
using WTG.Telematics.Common.Conversion;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;
using WTG.Telematics.Interfaces.Packets.V2;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class TirePressureReportPayloadRecordProcessorTest : TestCaseWithFactory
	{
		public void TestCreatesTirePressureReportReferencingAssignedOwnerAndTelSubEquiment()
		{
			// Arrange
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var dateTimeOffset = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10));
			var record = CreateMessage(dateTimeOffset, new[]
			{
				new TirePressureReportData()
				{
					TpmId = 1,
					DateTimeOffset = dateTimeOffset,
					Pressure = 10.2,
					RsaId = deviceIdMock.Object,
					Temperature = 50,
				}
			});
			var equipmentPk = new ZGuid();
			dbTreePlannerMock.Setup(
				planner => planner.GetSubEquipmentFromTree(
					It.IsAny<BusinessObjectFactory>(),
					It.IsAny<string>(),
					It.IsAny<DateTimeOffset>(),
					It.IsAny<string>()))
				.Returns(new Dictionary<string, SimpleSubEquipment> { { "1", new SimpleSubEquipment(equipmentPk, "1", "W") } });

			// Act
			processor.Process(Factory, glbDevice, record);

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have an onboard mass", 1, device.TyreReports.Count);

			var tirePressureReport = device.TyreReports[0];
			var tpmDeviceReference = Factory.Load<GlbDevice>(tirePressureReport.GDR_V3_Device);
			AssertEquals(new ZDecimal(50), tirePressureReport.GDR_TemperatureC);
			AssertEquals(dateTimeOffset.UtcDateTime, tirePressureReport.GDR_MeasurementTimeUtc);
			AssertEquals(new ZDecimal(10.2 * 6.895), tirePressureReport.GDR_PressureKPa);
			AssertEquals(equipmentPk, tirePressureReport.GDR_TSE_SubEquipment);
			AssertEquals(mobileServicesDeviceIdentifier, tpmDeviceReference.V3_MobileServicesIdentifier);
		}

		public void TestRecordsNotAddedForDeviceWithInactiveTpms()
		{
			// Arrange
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var dateTimeOffset = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10));
			var record = CreateMessage(dateTimeOffset, new[]
			{
				new TirePressureReportData()
				{
					TpmId = 1,
					DateTimeOffset = dateTimeOffset,
					Pressure = 10.2,
					RsaId = deviceIdMock.Object,
					Temperature = 50,
				}
			});
			dbTreePlannerMock.Setup(
					planner => planner.GetSubEquipmentFromTree(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<string>(),
						It.IsAny<DateTimeOffset>(),
						It.IsAny<string>()))
				.Returns(new Dictionary<string, SimpleSubEquipment>());

			// Act
			processor.Process(Factory, glbDevice, record);

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should not have an tpm report", 0, device.TyreReports.Count);
		}

		public void TestUnknownSubEquipmentWarningLoggedWhenTpmIsEnabledButNotDiscovered()
		{
			// Arrange
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var dateTimeOffset = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10));
			var record = CreateMessage(dateTimeOffset, new[]
			{
				new TirePressureReportData()
				{
					TpmId = 1,
					DateTimeOffset = dateTimeOffset,
					Pressure = 10.2,
					RsaId = deviceIdMock.Object,
					Temperature = 50,
				}
			});
			var equipmentPk = new ZGuid();
			dbTreePlannerMock.Setup(
					planner => planner.GetSubEquipmentFromTree(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<string>(),
						It.IsAny<DateTimeOffset>(),
						It.IsAny<string>()))
				.Returns(new Dictionary<string, SimpleSubEquipment> { { "2", new SimpleSubEquipment(equipmentPk, "2", "W") } });

			// Act
			processor.Process(Factory, glbDevice, record);

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should not have Tpm reports", 0, device.TyreReports.Count);
			AssertNull(ErrorReporter.LastExceptionReported);
			loggerMock.Verify(
				logger => logger.Log(
					LogType.Warning,
					$"Received data for unknown Subequipment [DeviceId: {BinaryDataConverter.ByteArrayToHexString(deviceIdentifier)}, Id: 1, Type: {TelSubEquipmentTypeList.Codes.Wheel}]."),
				Times.Once);
		}

		public void TestUnknownSubEquipmentWarningLoggedForMultipleIncorrectTpms22()
		{
			AssertUnknownSubEquipmentWarningLoggedForMultipleIncorrectTpms(2, 2, new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10)));
		}

		public void TestUnknownSubEquipmentWarningLoggedForMultipleIncorrectTpms510()
		{
			AssertUnknownSubEquipmentWarningLoggedForMultipleIncorrectTpms(5, 10, new DateTimeOffset(2021, 1, 1, 1, 1, 1, TimeSpan.FromHours(10)));
		}

		void AssertUnknownSubEquipmentWarningLoggedForMultipleIncorrectTpms(int numberOfValidEquipment, int numberOfInvalidEquipment, DateTimeOffset dateTimeOffset)
		{
			// Arrange
			loggerMock.Reset();
			dbTreePlannerMock.Reset();
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var record = CreateMessagesForDevices();
			dbTreePlannerMock.Setup(
					planner => planner.GetSubEquipmentFromTree(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<string>(),
						It.IsAny<DateTimeOffset>(),
						It.IsAny<string>()))
				.Returns(CreateSubEquipments());

			// Act
			processor.Process(Factory, glbDevice, record);

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should not have Tpm reports", numberOfValidEquipment, device.TyreReports.Count);
			AssertNull(ErrorReporter.LastExceptionReported);
			loggerMock.Verify(
				logger => logger.Log(
					LogType.Warning,
					It.IsAny<string>()),
				Times.Exactly(numberOfInvalidEquipment));

			TirePressureReportPayloadRecord CreateMessagesForDevices()
			{
				var reports = Enumerable.Range(0, numberOfValidEquipment)
					.Concat(Enumerable.Range(100, numberOfInvalidEquipment))
					.Select(i => new TirePressureReportData()
					{
						TpmId = (uint)i,
						DateTimeOffset = dateTimeOffset,
						Pressure = 10.2,
						RsaId = deviceIdMock.Object,
						Temperature = 50,
					});
				return CreateMessage(dateTimeOffset, reports);
			}

			Dictionary<string, SimpleSubEquipment> CreateSubEquipments()
			{
				return Enumerable.Range(0, numberOfValidEquipment)
					.Select(i => new SimpleSubEquipment(new ZGuid(), i.ToString(), "W"))
					.ToDictionary(equipment => equipment.Id);
			}
		}

		public void TestWrongParamsCall()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new TirePressureReportPayloadRecordProcessor(null, dbTreePlannerMock.Object));
			AssertEquals("logger", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new TirePressureReportPayloadRecordProcessor(loggerMock.Object, null));
			AssertEquals("dbTreePlanner", result.ParamName);
		}

		TirePressureReportPayloadRecord CreateMessage(DateTimeOffset dateTimeOffset, IEnumerable<TirePressureReportData> tpms)
		{
			return new TirePressureReportPayloadRecord()
			{
				DateTimeOffset = dateTimeOffset,
				Tpms = tpms,
			};
		}

		protected override void SetUp()
		{
			base.SetUp();

			mobileServicesDeviceIdentifier = new byte[] { 0x01, 0x04, 0x07, 0xFF }; // No significance
			deviceIdentifier = new byte[] { 0x02, 0x05, 0x08, 0x01 };

			glbDevice = Factory.New<GlbDevice>();
			glbDevice.V3_HumanReadableIdentifier = "TT00000001";
			glbDevice.V3_IsActive = true;
			glbDevice.V3_MobileServicesIdentifier = mobileServicesDeviceIdentifier;
			glbDevice.V3_HardwareIdentifier = BinaryDataConverter.ByteArrayToHexString(deviceIdentifier);
			glbDevice.V3_Model = "GLaDOS v3.1";
			devicePK = glbDevice.PK;

			deviceIdMock = new Mock<IDeviceId>();
			loggerMock = new Mock<ILogger>();
			dbTreePlannerMock = new Mock<IDbTreePlanner>();
			processor = new TirePressureReportPayloadRecordProcessor(loggerMock.Object, dbTreePlannerMock.Object);
		}

		byte[] mobileServicesDeviceIdentifier;
		byte[] deviceIdentifier;
		GlbDevice glbDevice;
		ZGuid devicePK;
		Mock<ILogger> loggerMock;
		TirePressureReportPayloadRecordProcessor processor;
		Mock<IDbTreePlanner> dbTreePlannerMock;
		Mock<IDeviceId> deviceIdMock;
	}
}
