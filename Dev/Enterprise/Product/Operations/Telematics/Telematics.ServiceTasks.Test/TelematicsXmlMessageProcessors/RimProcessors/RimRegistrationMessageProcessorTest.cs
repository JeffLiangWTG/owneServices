using System;
using System.Linq;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.RimProcessors
{
	class RimRegistrationMessageProcessorTest : RimRegistrationMessageProcessorTestBase
	{
		protected override void SetUp()
		{
			base.SetUp();
			processor = new RimRegistrationMessageProcessor(loggerMock.Object);
		}

		RimRegistrationMessageProcessor processor;

		public void TestRegisterRimDeviceCreatesNewRegistration()
		{
			SetupRefEquipmentAndGlbDevices(devices);
			CombineAssertions(() =>
			{
				Test(new RimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceAssignmentTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)),
				});
				Test(new RimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceAssignmentTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
			});

			void Test(RimRegistrationMessage message)
			{
				// Arrange
				// Act
				var result = processor.Process(Factory, message);

				// Assert
				AssertEquals(1, result);
				var glbDevice = Factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
				var registration = Factory.Load<TelEdge>(new ZQuery(
						new ZQuery(TelEdgeSchema.TE_RelationshipType, "RIM"),
						JoinCondition.And,
						new ZQuery(TelEdgeSchema.TE_EntityIdTo, glbDevice.PK)))
					.Single();
				AssertEquals(message.DeviceAssignmentTime, registration.TE_StartTime);
				AssertEquals(ZDateTimeOffset.Empty, registration.TE_EndTime);
			}
		}

		public void TestDuplicateRimRegistration()
		{
			SetupRefEquipmentAndGlbDevices(devices);
			CombineAssertions(() =>
			{
				Test(new RimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceAssignmentTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)),
				});
				Test(new RimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceAssignmentTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
			});

			void Test(RimRegistrationMessage message)
			{
				// Arrange
				// Act
				var firstResult = processor.Process(Factory, message);
				var secondResult = processor.Process(Factory, message);

				// Assert
				AssertEquals(1, firstResult);
				AssertEquals(0, secondResult);
				var glbDevice = Factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
				var registration = Factory.Load<TelEdge>(new ZQuery(
						new ZQuery(TelEdgeSchema.TE_RelationshipType, "RIM"),
						JoinCondition.And,
						new ZQuery(TelEdgeSchema.TE_EntityIdTo, glbDevice.PK)))
					.Single();
				AssertEquals(message.DeviceAssignmentTime, registration.TE_StartTime);
				AssertEquals(ZDateTimeOffset.Empty, registration.TE_EndTime);
			}
		}

		public void TestRegisterNonExistentRimDevice()
		{
			CombineAssertions(() =>
			{
				Test(new RimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceAssignmentTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)),
				});
				Test(new RimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceAssignmentTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
			});

			void Test(RimRegistrationMessage message)
			{
				// Arrange
				loggerMock.Reset();
				loggerMock.Setup(logger => logger.Log(LogType.Error, $"Device with HID: {message.DeviceId} does not have an associated GlbDevice"));

				// Act
				var result = processor.Process(Factory, message);

				// Assert
				loggerMock.Verify(logger => logger.Log(LogType.Error, $"Device with HID: {message.DeviceId} does not have an associated GlbDevice"), Times.Once);
				AssertEquals(0, result);
			}
		}

		public void TestRegisterRimDeviceWithoutLinkedEquipment()
		{
			var ids = devices.Select(tuple => tuple.id).ToList();
			SetupGlbDevices(ids);
			SetupRefEquipment(ids);
			CombineAssertions(() =>
			{
				Test(new RimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceAssignmentTime = new DateTimeOffset(2020, 1, 2, 3, 4, 5, 6, TimeSpan.FromHours(10)),
				});
				Test(new RimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceAssignmentTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
			});

			void Test(RimRegistrationMessage message)
			{
				// Arrange
				loggerMock.Reset();
				loggerMock.Setup(logger => logger.Log(LogType.Error, $"Device with HID: {message.DeviceId} does not have linked RefEquipment and GlbDevice"));

				// Act
				var result = processor.Process(Factory, message);

				// Assert
				loggerMock.Verify(logger => logger.Log(LogType.Error, $"Device with HID: {message.DeviceId} does not have linked RefEquipment and GlbDevice"), Times.Once);
				AssertEquals(0, result);
			}
		}

		public void TestWrongParamsCall()
		{
			// Arrange
			// Act
			// Assert
			var result = AssertExceptionThrown<ArgumentNullException>(() => new RimRegistrationMessageProcessor(null));
			AssertEquals("logger", result.ParamName);
		}
	}
}
