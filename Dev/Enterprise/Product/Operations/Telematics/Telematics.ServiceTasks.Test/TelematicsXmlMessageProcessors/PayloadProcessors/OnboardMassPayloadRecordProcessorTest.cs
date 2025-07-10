using System;
using System.Collections.Generic;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsEquipment;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.PayloadProcessors;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.Telematics.Common.Conversion;
using WTG.Telematics.Data.Packets.V2.Commercial.Ingoing;
using WTG.Telematics.Interfaces.Packets.V2;
using WTG.Telematics.Interfaces.Packets.V2.Commercial.Ingoing;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.PayloadProcessors
{
	public class OnboardMassPayloadRecordProcessorTest : TestCaseWithFactory
	{
		public void TestCreatesOnboardMassReferencingAssignedOwnerAndTelSubEquiment()
		{
			// Arrange
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var dateTimeOffset = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10));
			var record = CreateMessage(dateTimeOffset, new[]
			{
				new ObmData()
				{
					ObmId = 1,
					DateTimeOffset = dateTimeOffset,
					DeviationPercent = 31.2,
					Pressure = 10.2,
					RsaId = deviceIdMock.Object,
					Status = OnboardMassStatus.PressureSensor,
				}
			});
			var equipmentPk = new ZGuid();
			dbTreePlannerMock.Setup(
				planner => planner.GetSubEquipmentFromTree(
					It.IsAny<BusinessObjectFactory>(),
					It.IsAny<string>(),
					It.IsAny<DateTimeOffset>(),
					It.IsAny<string>()))
				.Returns(new Dictionary<string, SimpleSubEquipment> { { "01", new SimpleSubEquipment(equipmentPk, "01", "O") } });

			// Act
			processor.Process(Factory, glbDevice, record);

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should have an onboard mass", 1, device.OnboardMasses.Count);

			var onboardMass = device.OnboardMasses[0];
			var obmDeviceReference = Factory.Load<GlbDevice>(onboardMass.GDM_V3_Device);
			AssertEquals(new ZDecimal(31.2), onboardMass.GDM_DeviationPercent);
			AssertEquals(dateTimeOffset.UtcDateTime, onboardMass.GDM_MeasurementTimeUtc);
			AssertEquals(new ZDecimal(10.2 * 6.895), onboardMass.GDM_PressureKPa);
			AssertEquals(equipmentPk, onboardMass.GDM_TSE_SubEquipment);
			AssertEquals(mobileServicesDeviceIdentifier, obmDeviceReference.V3_MobileServicesIdentifier);
		}

		public void TestRecordsNotAddedForDeviceWithInactiveObms()
		{
			// Arrange
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var dateTimeOffset = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10));
			var record = CreateMessage(dateTimeOffset, new[]
			{
				new ObmData()
				{
					ObmId = 1,
					DateTimeOffset = dateTimeOffset,
					DeviationPercent = 31.2,
					Pressure = 10.2,
					RsaId = deviceIdMock.Object,
					Status = OnboardMassStatus.PressureSensor,
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
			AssertEquals("Device should have an onboard mass", 0, device.OnboardMasses.Count);
			loggerMock.Verify(logger => logger.Log(LogType.Warning, It.IsAny<string>()), Times.Never);
		}

		public void TestUnknownSubEquipmentWarningLoggedWhenObmIsEnabledButNotDiscovered()
		{
			// Arrange
			deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
			var dateTimeOffset = new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10));
			var record = CreateMessage(dateTimeOffset, new[]
			{
				new ObmData()
				{
					ObmId = 1,
					DateTimeOffset = dateTimeOffset,
					DeviationPercent = 31.2,
					Pressure = 10.2,
					RsaId = deviceIdMock.Object,
					Status = OnboardMassStatus.PressureSensor,
				}
			});
			var equipmentPk = new ZGuid();
			dbTreePlannerMock.Setup(
					planner => planner.GetSubEquipmentFromTree(
						It.IsAny<BusinessObjectFactory>(),
						It.IsAny<string>(),
						It.IsAny<DateTimeOffset>(),
						It.IsAny<string>()))
				.Returns(new Dictionary<string, SimpleSubEquipment> { { "02", new SimpleSubEquipment(equipmentPk, "02", "O") } });

			// Act
			processor.Process(Factory, glbDevice, record);

			// Assert
			var device = Factory.Load<GlbDevice>(devicePK);
			AssertEquals("Device should not have an onboard mass report", 0, device.OnboardMasses.Count);
			AssertNull(ErrorReporter.LastExceptionReported);
			loggerMock.Verify(
				logger => logger.Log(
					LogType.Warning,
					$"Received data for unknown Subequipment [DeviceId: {BinaryDataConverter.ByteArrayToHexString(deviceIdentifier)}, Id: 1, Type: {TelSubEquipmentTypeList.Codes.OBM}]."),
				Times.Once);
		}

		public void TestDuplicateRecordsAreDiscarded()
		{
			CombineAssertions(() =>
			{
				Test(100, new DateTimeOffset(2016, 1, 2, 3, 4, 5, TimeSpan.FromHours(11)), 2);
				Test(10.2, new DateTimeOffset(2020, 1, 1, 1, 1, 1, TimeSpan.FromHours(10)), 5);
			});

			void Test(double pressure, DateTimeOffset dateTimeOffset, int iterations)
			{
				// Arrange
				dbTreePlannerMock.Reset();
				deviceIdMock.Setup(id => id.ToBytes()).Returns(deviceIdentifier);
				loggerMock.Setup(logger => logger.Log(It.IsAny<LogType>(), It.IsAny<string>()));
				var record = CreateMessage(dateTimeOffset, new[]
				{
					new ObmData()
					{
						ObmId = 1,
						DateTimeOffset = dateTimeOffset,
						DeviationPercent = 31.2,
						Pressure = pressure,
						RsaId = deviceIdMock.Object,
						Status = OnboardMassStatus.PressureSensor,
					}
				});
				var equipmentPk = ZGuid.NewZGuid();
				dbTreePlannerMock.Setup(
						planner => planner.GetSubEquipmentFromTree(
							It.IsAny<BusinessObjectFactory>(),
							It.IsAny<string>(),
							It.IsAny<DateTimeOffset>(),
							It.IsAny<string>()))
					.Returns(new Dictionary<string, SimpleSubEquipment> { { "01", new SimpleSubEquipment(equipmentPk, "01", "O") } });

				// Act
				for (var i = 0; i < iterations; i++)
				{
					processor.Process(Factory, glbDevice, record);
				}

				// Assert
				var onboardMassRecords = Factory.Load<GlbDeviceOnboardMass>(new ZQuery(GlbDeviceOnboardMassSchema.GDM_TSE_SubEquipment, equipmentPk));
				AssertEquals(1, onboardMassRecords.Length);
				loggerMock.Verify(
					logger => logger.Log(LogType.Debug, $"Received duplicate data for Device: {BinaryDataConverter.ByteArrayToHexString(deviceIdentifier)} on Obm: 1 with value: {pressure} at time: {dateTimeOffset.ToString()}."),
					Times.Exactly(iterations - 1));
			}
		}

		public void TestWrongParamsCall()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new OnboardMassPayloadRecordProcessor(null, dbTreePlannerMock.Object));
			AssertEquals("logger", result.ParamName);

			result = AssertExceptionThrown<ArgumentNullException>(() => new OnboardMassPayloadRecordProcessor(loggerMock.Object, null));
			AssertEquals("dbTreePlanner", result.ParamName);
		}

		OnboardMassPayloadRecord CreateMessage(DateTimeOffset dateTimeOffset, IEnumerable<IObmData> obms)
		{
			return new OnboardMassPayloadRecord()
			{
				DateTimeOffset = dateTimeOffset,
				Obms = obms,
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
			processor = new OnboardMassPayloadRecordProcessor(loggerMock.Object, dbTreePlannerMock.Object);
		}

		byte[] mobileServicesDeviceIdentifier;
		byte[] deviceIdentifier;
		GlbDevice glbDevice;
		ZGuid devicePK;
		Mock<ILogger> loggerMock;
		OnboardMassPayloadRecordProcessor processor;
		Mock<IDbTreePlanner> dbTreePlannerMock;
		Mock<IDeviceId> deviceIdMock;
	}
}
