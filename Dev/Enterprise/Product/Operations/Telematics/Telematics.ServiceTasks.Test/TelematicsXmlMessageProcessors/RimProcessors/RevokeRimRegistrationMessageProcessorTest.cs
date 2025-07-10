using System;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.Telematics.Business;
using Enterprise.Telematics.ServiceTasks.TelematicsXmlMessageProcessors.RimProcessors;
using Enterprise.ZArchitecture.Schema;
using Moq;
using WTG.Telematics.Data.CargoWiseOne;

namespace Enterprise.Telematics.ServiceTasks.Test.TelematicsXmlMessageProcessors.RimProcessors
{
	class RevokeRimRegistrationMessageProcessorTest : RimRegistrationMessageProcessorTestBase
	{
		protected override void SetUp()
		{
			base.SetUp();
			processor = new RevokeRimRegistrationMessageProcessor(loggerMock.Object);
		}

		RevokeRimRegistrationMessageProcessor processor;

		public void TestRevokeRimRegistrationClosesPreviousRegistrations()
		{
			SetupRefEquipmentAndGlbDevices(devices);
			CombineAssertions(() =>
			{
				Test(new RevokeRimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceRevokeTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
				Test(new RevokeRimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceRevokeTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
			});

			void Test(RevokeRimRegistrationMessage message)
			{
				// Arrange
				var glbDevice = Factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
				var divot = Factory.LoadTop1<GlbDeviceAssignmentDivot>(new ZQuery(GlbDeviceAssignmentDivotSchema.V7_V3_Device, glbDevice.PK));
				var rimEdge = Factory.New<TelEdge>();
				rimEdge.TE_StartTime = new DateTimeOffset(divot.V7_StartTimeUtc.ToDateTime());
				rimEdge.TE_EntityIdTo = divot.V7_V3_Device;
				rimEdge.TE_EntityIdFrom = divot.V7_ParentID;
				rimEdge.TE_EntityTableCodeFrom = "RQ";
				rimEdge.TE_EntityTableCodeTo = "V3";
				rimEdge.TE_RelationshipType = TelEdgeRelationshipTypes.Codes.RIM;

				Factory.Save();

				// Act
				var result = processor.Process(Factory, message);

				// Assert
				AssertEquals(1, result);
				var edge = Factory.LoadTop1<TelEdge>(new ZQuery(
					new ZQuery(TelEdgeSchema.TE_RelationshipType, "RIM"),
					JoinCondition.And,
					new ZQuery(TelEdgeSchema.TE_EntityIdTo, glbDevice.PK)));
				AssertEquals(message.DeviceRevokeTime.UtcDateTime, edge.TE_EndTime.ToUtcDateTime());

				var registration = Factory.Load<TelEdge>(new ZQuery(
						new ZQuery(TelEdgeSchema.TE_RelationshipType, "RIM"),
						JoinCondition.And,
						new ZQuery(TelEdgeSchema.TE_EntityIdTo, glbDevice.PK)))
					.Single();
				AssertEquals(message.DeviceRevokeTime, registration.TE_EndTime.ToDateTimeOffset());
			}
		}

		public void TestDuplicateRimRevoke()
		{
			SetupRefEquipmentAndGlbDevices(devices);
			CombineAssertions(() =>
			{
				Test(new RevokeRimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceRevokeTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
				Test(new RevokeRimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceRevokeTime = new DateTimeOffset(2020, 6, 5, 4, 3, 2, 1, TimeSpan.FromHours(10)),
				});
			});

			void Test(RevokeRimRegistrationMessage message)
			{
				// Arrange
				var glbDevice = Factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
				var divot = Factory.LoadTop1<GlbDeviceAssignmentDivot>(new ZQuery(GlbDeviceAssignmentDivotSchema.V7_V3_Device, glbDevice.PK));
				var rimEdge = Factory.New<TelEdge>();
				rimEdge.TE_StartTime = new DateTimeOffset(divot.V7_StartTimeUtc.ToDateTime());
				rimEdge.TE_EntityIdTo = divot.V7_V3_Device;
				rimEdge.TE_EntityIdFrom = divot.V7_ParentID;
				rimEdge.TE_EntityTableCodeFrom = "RQ";
				rimEdge.TE_EntityTableCodeTo = "V3";
				rimEdge.TE_RelationshipType = TelEdgeRelationshipTypes.Codes.RIM;

				Factory.Save();

				// Act
				var firstResult = processor.Process(Factory, message);
				var secondResult = processor.Process(Factory, message);

				// Assert
				AssertEquals(1, firstResult);
				AssertEquals(0, secondResult);
				var edge = Factory.LoadTop1<TelEdge>(new ZQuery(
					new ZQuery(TelEdgeSchema.TE_RelationshipType, "RIM"),
					JoinCondition.And,
					new ZQuery(TelEdgeSchema.TE_EntityIdTo, glbDevice.PK)));
				AssertEquals(message.DeviceRevokeTime.UtcDateTime, edge.TE_EndTime.ToUtcDateTime());

				var registration = Factory.Load<TelEdge>(new ZQuery(
						new ZQuery(TelEdgeSchema.TE_RelationshipType, "RIM"),
						JoinCondition.And,
						new ZQuery(TelEdgeSchema.TE_EntityIdTo, glbDevice.PK)))
					.Single();
				AssertEquals(message.DeviceRevokeTime, registration.TE_EndTime.ToDateTimeOffset());
			}
		}

		public void TestInvalidRevokeTime()
		{
			SetupRefEquipmentAndGlbDevices(devices);
			CombineAssertions(() =>
			{
				Test(new RevokeRimRegistrationMessage
				{
					DeviceId = "01020304",
					DeviceRevokeTime = baseTime.AddHours(-5),
				});
				Test(new RevokeRimRegistrationMessage
				{
					DeviceId = "FFFFFFFF",
					DeviceRevokeTime = baseTime.AddDays(-2),
				});
			});

			void Test(RevokeRimRegistrationMessage message)
			{
				// Arrange
				var glbDevice = Factory.LoadTop1<GlbDevice>(new ZQuery(GlbDeviceSchema.V3_HardwareIdentifier, message.DeviceId));
				var divot = Factory.LoadTop1<GlbDeviceAssignmentDivot>(new ZQuery(GlbDeviceAssignmentDivotSchema.V7_V3_Device, glbDevice.PK));
				var rimEdge = Factory.New<TelEdge>();
				rimEdge.TE_StartTime = new DateTimeOffset(divot.V7_StartTimeUtc.ToDateTime());
				rimEdge.TE_EntityIdTo = divot.V7_V3_Device;
				rimEdge.TE_EntityIdFrom = divot.V7_ParentID;
				rimEdge.TE_EntityTableCodeFrom = "RQ";
				rimEdge.TE_EntityTableCodeTo = "V3";
				rimEdge.TE_RelationshipType = TelEdgeRelationshipTypes.Codes.RIM;
				Factory.Save();

				loggerMock.Setup(logger => logger.Log(LogType.Error, $"Invalid Rim Revoke for device:{message.DeviceId}. Attempted revoke at time {message.DeviceRevokeTime.ToString()} for device registered at {rimEdge.TE_StartTime}"));

				// Act
				var result = processor.Process(Factory, message);

				// Assert
				AssertEquals(0, result);
				loggerMock.Verify(
					logger => logger.Log(LogType.Error, $"Invalid Rim Revoke for device:{message.DeviceId}. Attempted revoke at time {message.DeviceRevokeTime.ToString()} for device registered at {rimEdge.TE_StartTime.ToString()}"),
					Times.Once);
			}
		}
	}
}
