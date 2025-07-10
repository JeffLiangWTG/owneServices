using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.MobileServices.Common;
using CargoWise.MobileServices.Common.Messages;
using CargoWise.MobileServices.Common.Messages.EHub;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.Messaging.Business;
using Enterprise.Messaging.Integration;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.Telematics.Business.Test
{
	[TestedType(typeof(GlbDevice))]
	class GlbDeviceTests : EnterpriseBusinessObjectTestCase
	{
	}

	class GlbDeviceBasicTests : TestCaseWithFactory
	{
		public void TestNoAuditLogs()
		{
			device.V3_HumanReadableIdentifier = "ABC123";
			device.V3_Model = "TestModel";
			Factory.Save();

			device.V3_Model = "TestModelChanged";
			Factory.Save();

			device.Delete();
			Factory.Save();

			CombineAssertions(() =>
			{
				Assert("An ADD audit log was created.", !device.Logs.HasLogWith(l => l.SL_SE_NKEvent == "ADD"));
				Assert("An EDT audit log was created.", !device.Logs.HasLogWith(l => l.SL_SE_NKEvent == "EDT"));
				Assert("A DEL audit log was created.", !device.Logs.HasLogWith(l => l.SL_SE_NKEvent == "DEL"));
			});
		}

		public void TestHumanReadableName()
		{
			device.V3_HumanReadableIdentifier = "ABC001";
			AssertEquals("ABC001", device.HumanReadableName);
		}

		public void TestHumanReadableIdentifierIsReadOnly()
		{
			Assert(device.V3_HumanReadableIdentifierInfo.ReadOnly);
		}

		public void TestClearAssignedParentClearsStaffParent()
		{
			device.AssignedParentStaffID = Guid.NewGuid();
			device.ClearAssignedParent();
			AssertEquals(ZGuid.Empty, device.AssignedParentStaffID);
		}

		public void TestClearAssignedParentClearsEquipmentParent()
		{
			device.AssignedParentEquipmentID = Guid.NewGuid();
			device.ClearAssignedParent();
			AssertEquals(ZGuid.Empty, device.AssignedParentEquipmentID);
		}

		public void TestSetsDeviceKeyNoOpsIfNull()
		{
			device.UpdateDeviceKey(null);

			AssertEquals(GlbDeviceKindCodes.Unknown, device.V3_HardwareKind);
			AssertEquals(ZString.Empty, device.V3_HardwareIdentifier);
		}

		public void TestSetsDeviceKeyOnBlankRecord()
		{
			device.UpdateDeviceKey(new DeviceKey { kind = DeviceKind.AppleMobile, identifier = "someiphone" });

			AssertEquals(GlbDeviceKindCodes.AppleMobile, device.V3_HardwareKind);
			AssertEquals("someiphone", device.V3_HardwareIdentifier);
		}

		public void TestUpdatesDeviceKeyOnUnknownRecord()
		{
			device.V3_HardwareKind = GlbDeviceKindCodes.Unknown;
			device.V3_HardwareIdentifier = "someiphone";

			device.UpdateDeviceKey(new DeviceKey { kind = DeviceKind.Android, identifier = "someiphone" });

			AssertEquals(GlbDeviceKindCodes.Android, device.V3_HardwareKind);
			AssertEquals("someiphone", device.V3_HardwareIdentifier);
		}

		public void TestUpdatesDeviceKeyOnCaseInsensitiveMatch()
		{
			device.V3_HardwareKind = GlbDeviceKindCodes.Unknown;
			device.V3_HardwareIdentifier = "spongebob";

			device.UpdateDeviceKey(new DeviceKey { kind = DeviceKind.WiseTechVehicularPlatform, identifier = "SpOnGeBoB" });

			AssertEquals(GlbDeviceKindCodes.WTGEmbedded, device.V3_HardwareKind);
			AssertEquals("spongebob", device.V3_HardwareIdentifier);
		}

		public void TestErrorUpdatingFixedDeviceKind()
		{
			device.V3_HardwareKind = GlbDeviceKindCodes.AppleMobile;
			device.V3_HardwareIdentifier = "someiphone";

			device.UpdateDeviceKey(new DeviceKey { kind = DeviceKind.Android, identifier = "someiphone" });

			AssertEquals(GlbDeviceKindCodes.AppleMobile, device.V3_HardwareKind);
			AssertEquals("someiphone", device.V3_HardwareIdentifier);

			AssertEquals("Attempted to reassign device (IOS, someiphone) to (AND, someiphone)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestErrorUpdatingFixedDeviceIdentifier()
		{
			device.V3_HardwareKind = GlbDeviceKindCodes.AppleMobile;
			device.V3_HardwareIdentifier = "someiphone";

			device.UpdateDeviceKey(new DeviceKey { kind = DeviceKind.AppleMobile, identifier = "anipad" });

			AssertEquals(GlbDeviceKindCodes.AppleMobile, device.V3_HardwareKind);
			AssertEquals("someiphone", device.V3_HardwareIdentifier);

			AssertEquals("Attempted to reassign device (IOS, someiphone) to (IOS, anipad)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestErrorUpdatingFixedDeviceKey()
		{
			device.V3_HardwareKind = GlbDeviceKindCodes.Unknown;
			device.V3_HardwareIdentifier = "someiphone";

			device.UpdateDeviceKey(new DeviceKey { kind = DeviceKind.Android, identifier = "galaxytab" });

			AssertEquals(GlbDeviceKindCodes.Unknown, device.V3_HardwareKind);
			AssertEquals("someiphone", device.V3_HardwareIdentifier);

			AssertEquals("Attempted to reassign device (UNK, someiphone) to (AND, galaxytab)", ErrorReporter.LastMessageReported);
			ErrorReporter.Clear();
		}

		public void TestIsRimRegistered()
		{
			device.V3_HardwareKind = GlbDeviceKindCodes.WTGEmbedded;
			device.V3_HardwareIdentifier = "01020304";
			device.V3_HumanReadableIdentifier = "TEL0000003";
			device.V3_IsActive = true;
			device.V3_Model = "Expendable Test Device";
			device.V3_MobileServicesIdentifier = new byte[] { 3 };

			CombineAssertions(() =>
			{
				Test(false, TelEdgeRelationshipTypes.Codes.HW, "TSE", "TSE", false);
				Test(false, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", true);
				Test(true, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", false);
			});

			void Test(bool readOnlyExpected, string relationshipType, string entityTableCodeFrom, string entityTableCodeTo, bool edgeClosed)
			{
				var telEdge = Factory.New<TelEdge>();
				telEdge.TE_RelationshipType = relationshipType;
				telEdge.TE_EntityIdFrom = device.AssignedParentEquipmentID;
				telEdge.TE_EntityIdTo = device.PK;
				telEdge.TE_EntityTableCodeFrom = entityTableCodeFrom;
				telEdge.TE_EntityTableCodeTo = entityTableCodeTo;
				telEdge.TE_StartTime = ZDateTimeOffset.Now;
				if (edgeClosed)
				{
					telEdge.TE_EndTime = ZDateTimeOffset.Now;
				}
				Factory.Save();

				AssertEquals(readOnlyExpected, device.V3_HardwareIdentifierInfo.ReadOnly);
			}
		}

		GlbDevice device;

		protected override void SetUp()
		{
			base.SetUp();

			device = Factory.New<GlbDevice>();
		}

		protected override void TearDown()
		{
			device = null;

			base.TearDown();
		}
	}

	class GlbDeviceAssignmentGetterTests : TestCaseWithFactory
	{
		public void TestBatmanDeviceParents()
		{
			var device = Factory.Load<GlbDevice>(primaryKeyOfBatmanDevice);
			AssertEquals(ZGuid.Empty, device.AssignedParentEquipmentID);
			AssertEquals(ZGuid.Empty, device.AssignedParentStaffID);
		}

		public void TestStaffDeviceParents()
		{
			var device = Factory.Load<GlbDevice>(primaryKeyOfDeviceWithStaffParent);
			AssertEquals(ZGuid.Empty, device.AssignedParentEquipmentID);
			AssertEquals(Env.CurrentUser.PK, device.AssignedParentStaffID);
		}

		public void TestEquipmentDeviceParents()
		{
			var device = Factory.Load<GlbDevice>(primaryKeyOfDeviceWithTelematicParent);
			AssertEquals(primaryKeyOfRefEquipment, device.AssignedParentEquipmentID);
			AssertEquals(ZGuid.Empty, device.AssignedParentStaffID);
		}

		public void TestV3_HardwareIdentifierReadOnly()
		{
			CombineAssertions(() =>
			{
				Test(false, TelEdgeRelationshipTypes.Codes.HW, "TSE", "TSE", false);
				Test(false, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", true);
				Test(true, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", false);
			});

			void Test(bool readOnlyExpected, string relationshipType, string entityTableCodeFrom, string entityTableCodeTo, bool edgeClosed)
			{
				var device = Factory.Load<GlbDevice>(primaryKeyOfDeviceWithTelematicParent);
				var telEdge = Factory.New<TelEdge>();
				telEdge.TE_RelationshipType = relationshipType;
				telEdge.TE_EntityIdFrom = device.AssignedParentEquipmentID;
				telEdge.TE_EntityIdTo = device.PK;
				telEdge.TE_EntityTableCodeFrom = entityTableCodeFrom;
				telEdge.TE_EntityTableCodeTo = entityTableCodeTo;
				telEdge.TE_StartTime = ZDateTimeOffset.Now;
				if (edgeClosed)
				{
					telEdge.TE_EndTime = ZDateTimeOffset.Now;
				}
				Factory.Save();

				AssertEquals(readOnlyExpected, device.V3_HardwareIdentifierInfo.ReadOnly);
			}
		}

		public void TestAssignedParentEquipmentIDReadOnly()
		{
			CombineAssertions(() =>
			{
				Test(false, TelEdgeRelationshipTypes.Codes.HW, "TSE", "TSE", false);
				Test(false, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", true);
				Test(true, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", false);
			});

			void Test(bool readOnlyExpected, string relationshipType, string entityTableCodeFrom, string entityTableCodeTo, bool edgeClosed)
			{
				var device = Factory.Load<GlbDevice>(primaryKeyOfDeviceWithTelematicParent);
				var telEdge = Factory.New<TelEdge>();
				telEdge.TE_RelationshipType = relationshipType;
				telEdge.TE_EntityIdFrom = device.AssignedParentEquipmentID;
				telEdge.TE_EntityIdTo = device.PK;
				telEdge.TE_EntityTableCodeFrom = entityTableCodeFrom;
				telEdge.TE_EntityTableCodeTo = entityTableCodeTo;
				telEdge.TE_StartTime = ZDateTimeOffset.Now;
				if (edgeClosed)
				{
					telEdge.TE_EndTime = ZDateTimeOffset.Now;
				}
				Factory.Save();

				AssertEquals(readOnlyExpected, device.AssignedParentEquipmentIDInfo.ReadOnly);
			}
		}

		public void TestV3_HardwareKindReadOnly()
		{
			CombineAssertions(() =>
			{
				Test(false, TelEdgeRelationshipTypes.Codes.HW, "TSE", "TSE", false);
				Test(false, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", true);
				Test(true, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", false);
			});

			void Test(bool readOnlyExpected, string relationshipType, string entityTableCodeFrom, string entityTableCodeTo, bool edgeClosed)
			{
				var device = Factory.Load<GlbDevice>(primaryKeyOfDeviceWithTelematicParent);
				var telEdge = Factory.New<TelEdge>();
				telEdge.TE_RelationshipType = relationshipType;
				telEdge.TE_EntityIdFrom = device.AssignedParentEquipmentID;
				telEdge.TE_EntityIdTo = device.PK;
				telEdge.TE_EntityTableCodeFrom = entityTableCodeFrom;
				telEdge.TE_EntityTableCodeTo = entityTableCodeTo;
				telEdge.TE_StartTime = ZDateTimeOffset.Now;
				if (edgeClosed)
				{
					telEdge.TE_EndTime = ZDateTimeOffset.Now;
				}
				Factory.Save();

				AssertEquals(readOnlyExpected, device.V3_ModelInfo.ReadOnly);
			}
		}

		public void TestV3_ModelReadOnly()
		{
			CombineAssertions(() =>
			{
				Test(false, TelEdgeRelationshipTypes.Codes.HW, "TSE", "TSE", false);
				Test(false, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", true);
				Test(true, TelEdgeRelationshipTypes.Codes.RIM, "RQ", "V3", false);
			});

			void Test(bool readOnlyExpected, string relationshipType, string entityTableCodeFrom, string entityTableCodeTo, bool edgeClosed)
			{
				var device = Factory.Load<GlbDevice>(primaryKeyOfDeviceWithTelematicParent);
				var telEdge = Factory.New<TelEdge>();
				telEdge.TE_RelationshipType = relationshipType;
				telEdge.TE_EntityIdFrom = device.AssignedParentEquipmentID;
				telEdge.TE_EntityIdTo = device.PK;
				telEdge.TE_EntityTableCodeFrom = entityTableCodeFrom;
				telEdge.TE_EntityTableCodeTo = entityTableCodeTo;
				telEdge.TE_StartTime = ZDateTimeOffset.Now;
				if (edgeClosed)
				{
					telEdge.TE_EndTime = ZDateTimeOffset.Now;
				}
				Factory.Save();

				AssertEquals(readOnlyExpected, device.V3_ModelInfo.ReadOnly);
			}
		}

		ZGuid primaryKeyOfBatmanDevice;
		ZGuid primaryKeyOfDeviceWithStaffParent;
		ZGuid primaryKeyOfDeviceWithTelematicParent;
		ZGuid primaryKeyOfRefEquipment;

		protected override void SetUp()
		{
			base.SetUp();

			var device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TEL0000001";
			device.V3_IsActive = true;
			device.V3_Model = "Expendable Test Device";
			device.V3_MobileServicesIdentifier = new byte[] { 1 };
			primaryKeyOfBatmanDevice = device.PK;

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TEL0000002";
			device.V3_IsActive = true;
			device.V3_Model = "Expendable Test Device";
			device.V3_MobileServicesIdentifier = new byte[] { 2 };

			var assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = ZDateTime.UtcNow;
			assignment.V7_ParentID = Env.CurrentUser.PK;
			assignment.V7_ParentTableCode = GlbStaffSchema.Constants.Prefix;
			assignment.V7_V3_Device = device.PK;

			device.AssignedParentStaffID = Env.CurrentUser.PK;
			primaryKeyOfDeviceWithStaffParent = device.PK;

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TEL0000003";
			device.V3_IsActive = true;
			device.V3_Model = "Expendable Test Device";
			device.V3_MobileServicesIdentifier = new byte[] { 3 };

			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_Registration = "DAT001";
			equipment.RQ_ShortCode = "DAT001";
			equipment.RQ_Description = "Robotic Arm That Tests Things";

			assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = ZDateTime.UtcNow;
			assignment.V7_ParentID = equipment.PK;
			assignment.V7_ParentTableCode = RefEquipmentSchema.Constants.Prefix;
			assignment.V7_V3_Device = device.PK;

			device.AssignedParentEquipmentID = primaryKeyOfRefEquipment = equipment.PK;
			primaryKeyOfDeviceWithTelematicParent = device.PK;

			Factory.Save();
		}
	}

	class GlbDeviceAutoAssignmentRecordCreationTests : TestCaseWithFactory
	{
		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestDoesNotCloseAssignmentWhenDeviceLoadedAndSaved()
		{
			device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;
			Factory.Save();

			var newFactory = new BusinessObjectFactory();
			var deviceFromNewFactory = newFactory.Load<GlbDevice>(device.PK);
			deviceFromNewFactory.V3_Model = "Version 2.0!";
			newFactory.Save();

			Factory.ReloadAll<GlbDevice>();

			var assignment = device.Assignments.Single();
			AssertEquals("Assignment should remain open after saving device.", true, assignment.V7_EndTimeUtc.IsEmpty);
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestCreatesStaffAssignmentWhenAssignedAndSaved()
		{
			device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;
			AssertEquals("Should have no assignments before saving.", 0, device.Assignments.Count);

			Factory.Save();
			AssertEquals("Should have one assignment after saving.", 1, device.Assignments.Count);

			var assignment = device.Assignments[0];
			AssertEquals(GlbStaffSchema.Constants.Prefix, assignment.V7_ParentTableCode);
			AssertEquals(PrimaryKeyOfCurrentUser, assignment.V7_ParentID);
			AssertEquals(device.PK, assignment.V7_V3_Device);

			AssertEquals(dateTimeUtcOfTest, assignment.V7_StartTimeUtc);
			AssertEquals(ZDateTime.Empty, assignment.V7_EndTimeUtc);
		}

		public void TestCreatesMessageForStaffWhenAssignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;
				Factory.Save();

				AssertSavedEDIInterchangeForStaff();
			}
		}

		public void TestCreatesNoMessageForStaffWhenAssignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;
				Factory.Save();

				AssertSavedNoEDIInterchangeForStaff();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestCreatesEquipmentAssignmentWhenAssignedAndSaved()
		{
			device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
			AssertEquals("Should have no assignments before saving.", 0, device.Assignments.Count);

			Factory.Save();
			AssertEquals("Should have one assignment after saving.", 1, device.Assignments.Count);

			var assignment = device.Assignments[0];
			AssertEquals(RefEquipmentSchema.Constants.Prefix, assignment.V7_ParentTableCode);
			AssertEquals(PrimaryKeyOfEquipment, assignment.V7_ParentID);
			AssertEquals(device.PK, assignment.V7_V3_Device);

			AssertEquals(dateTimeUtcOfTest, assignment.V7_StartTimeUtc);
			AssertEquals(ZDateTime.Empty, assignment.V7_EndTimeUtc);
		}

		public void TestCreatesMessageForEquipmentWhenAssignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
				Factory.Save();

				AssertSavedEDIInterchangeForEquipment();
			}
		}

		public void TestCreatesNoMessageForEquipmentWhenAssignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
				Factory.Save();

				AssertSavedNoEDIInterchangeForEquipment();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestClosesExistingStaffAssignmentWhenDeassignedAndSaved()
		{
			CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfCurrentUser);
			device.AssignedParentStaffID = ZGuid.Empty;

			Factory.Save();
			AssertEquals("Should have one assignment after saving.", 1, device.Assignments.Count);

			var assignment = device.Assignments[0];
			AssertEquals(GlbStaffSchema.Constants.Prefix, assignment.V7_ParentTableCode);
			AssertEquals(PrimaryKeyOfCurrentUser, assignment.V7_ParentID);
			AssertEquals(device.PK, assignment.V7_V3_Device);

			AssertEquals(dateTimeUtcOfTest.AddMonths(-1), assignment.V7_StartTimeUtc);
			AssertEquals(dateTimeUtcOfTest, assignment.V7_EndTimeUtc);
		}

		public void TestCreatesMessageForStaffWhenDeassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfCurrentUser);
				device.AssignedParentStaffID = ZGuid.Empty;

				Factory.Save();

				AssertSavedEDIInterchangeForBlanking();
			}
		}

		public void TestCreatesNoMessageForStaffWhenDeassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfCurrentUser);
				device.AssignedParentStaffID = ZGuid.Empty;

				Factory.Save();

				AssertSavedNoEDIInterchangeForBlanking();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestClosesExistingEquipmentAssignmentWhenDeassignedAndSaved()
		{
			CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfEquipment);
			device.AssignedParentEquipmentID = ZGuid.Empty;

			Factory.Save();
			AssertEquals("Should have one assignment after saving.", 1, device.Assignments.Count);

			var assignment = device.Assignments[0];
			AssertEquals(RefEquipmentSchema.Constants.Prefix, assignment.V7_ParentTableCode);
			AssertEquals(PrimaryKeyOfEquipment, assignment.V7_ParentID);
			AssertEquals(device.PK, assignment.V7_V3_Device);

			AssertEquals(dateTimeUtcOfTest.AddMonths(-1), assignment.V7_StartTimeUtc);
			AssertEquals(dateTimeUtcOfTest, assignment.V7_EndTimeUtc);
		}

		public void TestCreatesMessageForEquipmentWhenDeassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfEquipment);
				device.AssignedParentEquipmentID = ZGuid.Empty;
				Factory.Save();

				AssertSavedEDIInterchangeForBlanking();
			}
		}

		public void TestCreatesNoMessageForEquipmentWhenDeassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfEquipment);
				device.AssignedParentEquipmentID = ZGuid.Empty;
				Factory.Save();

				AssertSavedNoEDIInterchangeForBlanking();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestClosesExistingAssignmentAndCreatesNewStaffAssignmentWhenReassignedAndSaved()
		{
			CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser);
			device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;

			Factory.Save();

			AssertReassignedDevice(device, dateTimeUtcOfTest.AddMonths(-1), dateTimeUtcOfTest, GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser, GlbStaffSchema.Constants.Prefix, PrimaryKeyOfCurrentUser);
		}

		public void TestCreatesMessageForStaffWhenReassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser);
				device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;

				Factory.Save();

				AssertSavedEDIInterchangeForStaff();
			}
		}

		public void TestCreatesNoMessageForStaffWhenReassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser);
				device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;

				Factory.Save();

				AssertSavedNoEDIInterchangeForStaff();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestClosesExistingAssignmentAndCreatesNewEquipmentAssignmentWhenReassignedAndSaved()
		{
			CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment);
			device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;

			Factory.Save();

			AssertReassignedDevice(device, dateTimeUtcOfTest.AddMonths(-1), dateTimeUtcOfTest, RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment, RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfEquipment);
		}

		public void TestCreatesMessageForEquipmentWhenReassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment);
				device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
				Factory.Save();

				AssertSavedEDIInterchangeForEquipment();
			}
		}

		public void TestCreatesNoMessageForEquipmentWhenReassignedAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment);
				device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
				Factory.Save();

				AssertSavedNoEDIInterchangeForEquipment();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestClosesExistingAssignmentAndCreatesNewStaffAssignmentWhenReassignedFromStaffToEquipmentAndSaved()
		{
			CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser);
			device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;

			Factory.Save();

			AssertReassignedDevice(device, dateTimeUtcOfTest.AddMonths(-1), dateTimeUtcOfTest, GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser, RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfEquipment);
		}

		public void TestCreatesMessageForEquipmentWhenReassignedFromStaffToEquipmentAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser);
				device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
				Factory.Save();

				AssertSavedEDIInterchangeForEquipment();
			}
		}

		public void TestCreatesNoMessageForEquipmentWhenReassignedFromStaffToEquipmentAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(GlbStaffSchema.Constants.Prefix, PrimaryKeyOfSecondaryUser);
				device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
				Factory.Save();

				AssertSavedNoEDIInterchangeForEquipment();
			}
		}

		[TestDate(2015, 03, 23, 9, 43, 12)]
		public void TestClosesExistingAssignmentAndCreatesNewEquipmentAssignmentWhenReassignedFromEquipmentToStaffAndSaved()
		{
			CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment);
			device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;

			Factory.Save();

			AssertReassignedDevice(device, dateTimeUtcOfTest.AddMonths(-1), dateTimeUtcOfTest, RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment, GlbStaffSchema.Constants.Prefix, PrimaryKeyOfCurrentUser);
		}

		public void TestCreatesMessageForStaffWhenReassignedFromEquipmentToStaffAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier();
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment);
				device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;

				Factory.Save();

				AssertSavedEDIInterchangeForStaff();
			}
		}

		public void TestCreatesNoMessageForStaffWhenReassignedFromEquipmentToStaffAndSaved()
		{
			var telematicsNotifier = new ShouldNotifySynchronouslyTelematicsNotifier(false);
			using (ObjectFactory.Substitute<ITelematicsNotifier>(telematicsNotifier))
			{
				CreateOpenAssignment(RefEquipmentSchema.Constants.Prefix, PrimaryKeyOfSecondaryEquipment);
				device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;

				Factory.Save();

				AssertSavedNoEDIInterchangeForStaff();
			}
		}

		public void TestCreatesAssignmentForMostRecentlyAssignedStaff()
		{
			var numAssignments = device.Assignments.Count;

			device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;
			Factory.Save();
			AssertEquals("Assignment count should have a delta of 1 after assigning.", 1, device.Assignments.Count - numAssignments);

			device.ClearAssignedParent();
			Factory.Save();
			AssertEquals("Assignment count should have a delta of 1 after assigning, then clearing.", 1, device.Assignments.Count - numAssignments);

			device.AssignedParentStaffID = PrimaryKeyOfCurrentUser;
			Factory.Save();
			AssertEquals("Assignment count should have a delta of 2 after assigning, then clearing, then re-assigning.", 2, device.Assignments.Count - numAssignments);
		}

		public void TestCreatesAssignmentForMostRecentlyAssignedEquipment()
		{
			var numAssignments = device.Assignments.Count;

			device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
			Factory.Save();
			AssertEquals("Assignment count should have a delta of 1 after assigning.", 1, device.Assignments.Count - numAssignments);

			device.ClearAssignedParent();
			Factory.Save();
			AssertEquals("Assignment count should have a delta of 1 after assigning, then clearing.", 1, device.Assignments.Count - numAssignments);

			device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
			Factory.Save();
			AssertEquals("Assignment count should have a delta of 2 after assigning, then clearing, then re-assigning.", 2, device.Assignments.Count - numAssignments);
		}

		public void TestCreatesAssignmentToNonexistentEquipment()
		{
			// Arrange
			var missingGuid = ZGuid.NewZGuid();
			device.AssignedParentEquipmentID = PrimaryKeyOfEquipment;
			Factory.Save();

			// Act
			device.AssignedParentEquipmentID = missingGuid;
			Factory.Save();

			// Assert
			AssertEquals(missingGuid, device.AssignedParentEquipmentID);
			AssertEquals(ZString.Empty, device.AssignedParentName);
		}

		public void TestCreatesAssignmentToNonexistentStaff()
		{
			// Arrange
			var missingGuid = ZGuid.NewZGuid();
			device.AssignedParentStaffID = PrimaryKeyOfEquipment;
			Factory.Save();

			// Act
			device.AssignedParentStaffID = missingGuid;
			Factory.Save();

			// Assert
			AssertEquals(missingGuid, device.AssignedParentStaffID);
			AssertEquals(ZString.Empty, device.AssignedParentName);
		}

		public void TestFindBYODByHumanReadableIdentifier()
		{
			AssertNull(GlbDevice.FindBYODByHumanReadableIdentifier(Factory, "VAS3390"));

			var device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "VAS3390";
			AssertNull(GlbDevice.FindBYODByHumanReadableIdentifier(Factory, "VAS3390"));

			device.V3_IsBYOD = true;
			AssertEquals(device, GlbDevice.FindBYODByHumanReadableIdentifier(Factory, "VAS3390"));
		}

		GlbDevice device;
		ZGuid PrimaryKeyOfCurrentUser;
		ZGuid PrimaryKeyOfEquipment;
		ZGuid PrimaryKeyOfSecondaryUser;
		ZGuid PrimaryKeyOfSecondaryEquipment;
		readonly DateTime dateTimeUtcOfTest = new DateTime(2015, 03, 23, 9, 43, 12, DateTimeKind.Utc);

		static void AssertReassignedDevice(GlbDevice device, ZDateTime initialDateTime, ZDateTime dateTimeOfReassignment, ZString initialParentTableCode, ZGuid initialParentID, ZString newParentTableCode, ZGuid newParentID)
		{
			var assignments = device.Assignments.OrderBy(a => a.V7_StartTimeUtc).ToList();
			AssertEquals("Should have two assignments after saving.", 2, assignments.Count);

			var index = 0;
			var assignment = assignments[index++];
			AssertEquals(initialParentTableCode, assignment.V7_ParentTableCode);
			AssertEquals(initialParentID, assignment.V7_ParentID);
			AssertEquals(device.PK, assignment.V7_V3_Device);

			AssertEquals(initialDateTime, assignment.V7_StartTimeUtc);
			AssertEquals(dateTimeOfReassignment, assignment.V7_EndTimeUtc);

			assignment = assignments[index++];
			AssertEquals(newParentTableCode, assignment.V7_ParentTableCode);
			AssertEquals(newParentID, assignment.V7_ParentID);
			AssertEquals(device.PK, assignment.V7_V3_Device);

			AssertEquals(dateTimeOfReassignment, assignment.V7_StartTimeUtc);
			AssertEquals(ZDateTime.Empty, assignment.V7_EndTimeUtc);
		}

		void AssertSavedEDIInterchangeWithAssignedDeviceInfo(string deviceHumanReadableIdentifier, string parentCode, string parentDescription, string parentType)
		{
			var interchange = GetEdiInterchanges(deviceHumanReadableIdentifier, parentCode, parentDescription, parentType).First();

			AssertNotNull("Interchange should have been created", interchange);

			AssertEquals("Interchange sender should be current licence code", Env.CurrentCompany.GetLicenceCode(), interchange.EI_From);
			AssertEquals("Interchange should be sent to ediProd (licence identifier)", SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value, interchange.EI_To);
			AssertEquals("Interchange should be active", true, interchange.EI_IsActive);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, interchange.EI_ReceiveTransmit);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, interchange.EI_Status);
			AssertEquals(EDIInterchangeTransportTypeList.Codes.eHub, interchange.EI_TransportType);

			var messages = interchange.ContainedMessages;
			AssertEquals("Interchange should have one message", 1, messages.Count);

			var message = messages[0];
			AssertEquals("Message should be sent to ediProd (licence identifier)", SystemDataRegistry.Instance.EdiProdLicenceIdentifier.Value, interchange.EI_To);
			AssertEquals("Message should be active", true, message.EM_IsActive);
			AssertEquals(ReceiveTransmitList.Codes.Transmit, message.EM_ReceiveTransmit);
			AssertEquals(EDIInterchangeStatusList.Codes.eHubQueued, message.EM_Status);
			AssertEquals(ApplicationCodeList.Codes.Telematics, message.EM_ApplicationCode);
			AssertEquals(TelematicsMessageList.Codes.ProtobufData, message.EM_MessageSubType);

			var readers = new[]
			{
				interchange.GetEI_BodyTextReader(),
				message.GetEM_MessageTextReader()
			};
			try
			{
				foreach (var reader in readers)
				{
					XElement messageBody = XElement.Load(reader);

					var serviceMessages = EHubMessageSerializer.Deserialize(messageBody).ToList();
					AssertEquals(1, serviceMessages.Count);

					var serviceMessage = serviceMessages[0];
					AssertEquals(EHubMessageType.C2WDeviceAssignedToUserNotification, serviceMessage.message_type);

					var innerServiceMessage = serviceMessage.GetInternalMessage<C2WDeviceAssignedToUserNotificationMessage>();
					AssertEquals(deviceHumanReadableIdentifier, innerServiceMessage.device_friendly_identifier);
					AssertEquals(parentCode, innerServiceMessage.parent_code);
					AssertEquals(parentDescription, innerServiceMessage.parent_description);
					AssertEquals(parentType, innerServiceMessage.parent_type);
				}
			}
			finally
			{
				foreach (var reader in readers)
				{
					reader.Dispose();
				}
			}
		}

		void AssertSavedNoEDIInterchangeWithAssignedDeviceInfo(string deviceHumanReadableIdentifier, string parentCode, string parentDescription, string parentType)
		{
			Assert(!GetEdiInterchanges(deviceHumanReadableIdentifier, parentCode, parentDescription, parentType).Any());
		}

		IEnumerable<EDIInterchange> GetEdiInterchanges(string deviceHumanReadableIdentifier, string parentCode, string parentDescription, string parentType)
		{
			var telematicsQuery = new ZQuery(EDIInterchangeSchema.EI_ApplicationCode, SQLComparisonOperator.Equal, ApplicationCodeList.Codes.Telematics);
			var transmitQuery = new ZQuery(EDIInterchangeSchema.EI_ReceiveTransmit, SQLComparisonOperator.Equal, ReceiveTransmitList.Codes.Transmit);
			var query = new ZQuery(telematicsQuery, JoinCondition.And, transmitQuery) { OrderBy = EDIInterchangeSchema.Constants.EI_SystemCreateTimeUtc };

			return Factory.Load<EDIInterchange>(query).OrderByDescending(x => x.EI_SystemCreateTimeUtc);
		}

		void CreateOpenAssignment(ZString parentTableCode, ZGuid parentID)
		{
			var assignment = Factory.New<GlbDeviceAssignmentDivot>();
			assignment.V7_StartTimeUtc = dateTimeUtcOfTest.AddMonths(-1);
			assignment.V7_ParentID = parentID;
			assignment.V7_ParentTableCode = parentTableCode;
			assignment.V7_V3_Device = device.PK;
			Factory.Save();
		}

		void AssertSavedEDIInterchangeForEquipment()
		{
			AssertSavedEDIInterchangeWithAssignedDeviceInfo("TEL0000001", "DAT001", "Robotic Arm That Tests Things", RefEquipmentSchema.Constants.Prefix);
		}

		void AssertSavedNoEDIInterchangeForEquipment()
		{
			AssertSavedNoEDIInterchangeWithAssignedDeviceInfo("TEL0000001", "DAT001", "Robotic Arm That Tests Things", RefEquipmentSchema.Constants.Prefix);
		}

		void AssertSavedEDIInterchangeForStaff()
		{
			AssertSavedEDIInterchangeWithAssignedDeviceInfo("TEL0000001", Env.CurrentUser.Initials, Env.CurrentUser.FullName, GlbStaffSchema.Constants.Prefix);
		}

		void AssertSavedNoEDIInterchangeForStaff()
		{
			AssertSavedNoEDIInterchangeWithAssignedDeviceInfo("TEL0000001", Env.CurrentUser.Initials, Env.CurrentUser.FullName, GlbStaffSchema.Constants.Prefix);
		}

		void AssertSavedEDIInterchangeForBlanking()
		{
			AssertSavedEDIInterchangeWithAssignedDeviceInfo("TEL0000001", string.Empty, string.Empty, string.Empty);
		}

		void AssertSavedNoEDIInterchangeForBlanking()
		{
			AssertSavedNoEDIInterchangeWithAssignedDeviceInfo("TEL0000001", string.Empty, string.Empty, string.Empty);
		}

		protected override void SetUp()
		{
			base.SetUp();

			PrimaryKeyOfCurrentUser = Env.CurrentUser.PK;

			device = Factory.New<GlbDevice>();
			device.V3_HumanReadableIdentifier = "TEL0000001";
			device.V3_IsActive = true;
			device.V3_Model = "Expendable Test Device";

			var equipment = Factory.New<RefEquipment>();
			equipment.RQ_Registration = "DAT001";
			equipment.RQ_ShortCode = "DAT001";
			equipment.RQ_Description = "Robotic Arm That Tests Things";
			PrimaryKeyOfEquipment = equipment.PK;

			equipment = Factory.New<RefEquipment>();
			equipment.RQ_Registration = "DAT002";
			equipment.RQ_ShortCode = "DAT002";
			equipment.RQ_Description = "Robotic Arm That Breaks Things";
			PrimaryKeyOfSecondaryEquipment = equipment.PK;

			PrimaryKeyOfSecondaryUser = Factory.Load<GlbStaff>(new ZQuery(GlbStaffSchema.PK, SQLComparisonOperator.NotEqual, PrimaryKeyOfCurrentUser))
				.Select(s => s.PK)
				.First();

			Factory.Save();
		}

		class ShouldNotifySynchronouslyTelematicsNotifier : ITelematicsNotifier
		{
			public ShouldNotifySynchronouslyTelematicsNotifier(bool souldNotifySynchronously = true)
			{
				this.souldNotifySynchronously = souldNotifySynchronously;
				inner = ObjectFactory.Get<ITelematicsNotifier>();
			}

			public bool ShouldNotifySynchronously => souldNotifySynchronously;

			public void NotifyMobileServicesOfNewDeviceParent(BusinessObjectFactory factory, string deviceFriendlyIdentifier, string parentCode, string parentDescription, string parentType) => inner.NotifyMobileServicesOfNewDeviceParent(factory, deviceFriendlyIdentifier, parentCode, parentDescription, parentType);

			public void NotifyTelematicsServicesOfDeviceDetails(BusinessObjectFactory factory, IEnumerable<string> telematicsClientIds, string humanReadableIdentifier, string model, string hardwareIdentifier, string deviceWasAssignedTo, string deviceAssignedTo) => inner.NotifyTelematicsServicesOfDeviceDetails(factory, telematicsClientIds, humanReadableIdentifier, model, hardwareIdentifier, deviceWasAssignedTo, deviceAssignedTo);

			public void NotifyMobileServicesOfDeviceDetails(BusinessObjectFactory factory, string mobileServicesClientId, string deviceFriendlyIdentifier, bool isBYOD, string manufacturer, string description, DeviceKind deviceKind, string deviceDrivenIdentifier, string clientLicenceCode) => inner.NotifyMobileServicesOfDeviceDetails(factory, mobileServicesClientId, deviceFriendlyIdentifier, isBYOD, manufacturer, description, deviceKind, deviceDrivenIdentifier, clientLicenceCode);

			public void NotifyMobileServicesOfBYODRegistration(BusinessObjectFactory factory, string deviceClientIdentifier, string deviceModel, DeviceKind deviceKind, string deviceIdentifier) => inner.NotifyMobileServicesOfBYODRegistration(factory, deviceClientIdentifier, deviceModel, deviceKind, deviceIdentifier);

			public void NotifyMobileServicesOfBYODDeregistration(BusinessObjectFactory factory, string deviceClientIdentifier) => inner.NotifyMobileServicesOfBYODDeregistration(factory, deviceClientIdentifier);

			readonly ITelematicsNotifier inner;
			readonly bool souldNotifySynchronously;
		}
	}
}
