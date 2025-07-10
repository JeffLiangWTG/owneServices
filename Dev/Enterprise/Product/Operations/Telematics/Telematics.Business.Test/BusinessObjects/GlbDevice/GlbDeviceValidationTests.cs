using System;
using System.Linq;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.Telematics.Business.Test
{
	class GlbDeviceValidationTests : BusinessObjectValidationTestCase
	{
		public void TestAssignedParentStaffIDCanBeEmpty()
		{
			device.AssignedParentStaffID = ZGuid.Empty;
			validation.ValidateAll();
			AssertEquals(false, device.HasErrors);
		}

		public void TestAssignedParentStaffIDMustBeValid()
		{
			device.AssignedParentStaffID = ZGuid.Invalid;
			validation.ValidateAll();
			AssertEquals(true, device.HasErrors);
		}

		public void TestAssignedParentStaffIDCanHaveMultipleOpenDivots()
		{
			// Arrange
			var staff = Factory.NewWithValidTestData(typeof(GlbStaff));
			SetupMultipleOpenDivotsOnAssignedParent("GS", staff.PK);

			// Act
			device.AssignedParentStaffID = staff.PK;
			validation.ValidateAll();

			// Assert
			AssertEquals(false, device.HasErrors);
		}

		public void TestAssignedParentEquipmentIDCanBeEmpty()
		{
			device.AssignedParentEquipmentIDInfo.ClearValue();
			validation.ValidateAll();
			AssertEquals(false, device.HasErrors);
		}

		public void TestAssignedParentEquipmentIDMustBeValid()
		{
			device.AssignedParentEquipmentID = ZGuid.Invalid;
			validation.ValidateAll();
			AssertEquals(true, device.HasErrors);
		}

		public void TestAssignedParentEquipmentCannotHaveAnExistingOpenDivot()
		{
			// Arrange
			var parent = new ZGuid(Guid.NewGuid());
			SetupMultipleOpenDivotsOnAssignedParent("RQ", parent);

			// Act
			device.AssignedParentEquipmentID = parent;
			validation.ValidateAll();

			// Assert
			var notification = device.AssignedParentEquipmentIDInfo.Notifications
				.Single(n => n.Message.Equals("The Equipment you are trying to assign this device to is currently assigned to SomeDevice"));
			AssertEquals(true, device.HasErrors);
			AssertEquals("The Equipment you are trying to assign this device to is currently assigned to SomeDevice", notification.Message);
		}

		void SetupMultipleOpenDivotsOnAssignedParent(string parentTableCode, ZGuid fakeParentGuid)
		{
			var device2 = Factory.New<GlbDevice>();
			device2.V3_HumanReadableIdentifier = "SomeDevice";

			var preExistingDivot = Factory.New<GlbDeviceAssignmentDivot>();
			preExistingDivot.V7_V3_Device = device2.PK;
			preExistingDivot.V7_ParentID = fakeParentGuid;
			preExistingDivot.V7_StartTimeUtc = ZDateTime.BrettsBirthday;
			preExistingDivot.V7_ParentTableCode = parentTableCode;
		}

		GlbDeviceValidation validation;
		GlbDevice device;

		protected override void SetUp()
		{
			base.SetUp();

			device = Factory.New<GlbDevice>();
			device.V3_MobileServicesIdentifier = new byte[] { 0x00 };
			validation = device.Validation;

			validation.ValidateAll();
			AssertEquals("Sanity check - should have no errors at end of test setup", false, device.HasErrors);
		}
	}
}
