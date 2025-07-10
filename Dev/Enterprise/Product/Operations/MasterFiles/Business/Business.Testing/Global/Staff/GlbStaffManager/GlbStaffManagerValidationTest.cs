using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.Registry.Business;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	sealed class GlbStaffManagerValidationTest : BusinessObjectValidationTestCase
	{
		[TestDate(2019, 04, 04)]
		public void TestGSM_ManagerTypeSharedRoles()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();

			var manager1HRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01));
			AssertNoWarnings(manager1HRMRecord.GSM_ManagerTypeInfo);
			var manager1PRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 01, 01));
			AssertNoWarnings(manager1PRMRecord.GSM_ManagerTypeInfo);
			Factory.Save();

			var manager2HRMRecord = StaffManagerTestHelper.AddManager(staff, manager2, "HRM", ZDate.Today);
			AssertNoWarnings(manager2HRMRecord.GSM_ManagerTypeInfo);

			var manager2PRMRecord = StaffManagerTestHelper.AddManager(staff, manager2, "PRM", ZDate.Today);
			AssertHasWarning(manager2PRMRecord.GSM_ManagerTypeInfo, manager2PRMRecord.Validation.CannotShareRoleMessage);

			manager1PRMRecord.GSM_EndDate = new ZDateTime(2019, 04, 03);
			manager2PRMRecord.RunPreSaveValidation();
			AssertNoWarnings(manager2PRMRecord.GSM_ManagerTypeInfo);

			manager1PRMRecord.GSM_EndDate = new ZDateTime(2019, 04, 04);
			manager2PRMRecord.RunPreSaveValidation();
			AssertHasWarning(manager2PRMRecord.GSM_ManagerTypeInfo, manager2PRMRecord.Validation.CannotShareRoleMessage);

			manager2PRMRecord.GSM_EffectiveDate = new ZDateTime(2018, 04, 04);
			manager2PRMRecord.GSM_EndDate = new ZDateTime(2018, 12, 31);
			AssertNoWarnings(manager2PRMRecord.GSM_ManagerTypeInfo);

			manager2PRMRecord.GSM_EndDate = new ZDateTime(2019, 01, 01);
			AssertHasWarning(manager2PRMRecord.GSM_ManagerTypeInfo, manager2PRMRecord.Validation.CannotShareRoleMessage);
		}

		public void TestGSM_ManagerTypeDisabledRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var disabledManagerRecord = StaffManagerTestHelper.AddManager(staff, manager, "TRM");
			AssertHasError(disabledManagerRecord.GSM_ManagerTypeInfo, "Enter a valid selection.");
		}

		public void TestGSM_EffectiveDate()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM");
			managerRecord.GSM_EffectiveDate = ZDateTime.Empty;
			AssertHasError(managerRecord.GSM_EffectiveDateInfo, "An Effective Date must be entered.");

			managerRecord.GSM_ManagerType = DefaultStaffReportingRoles.Codes.DirectManager;
			AssertHasErrors(managerRecord.GSM_EffectiveDateInfo);
		}

		public void TestGSM_EffectiveDateMustBeCurrent()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM");
			AssertEquals("Precondition", false, managerRecord.MustBeCurrent);
			managerRecord.GSM_EffectiveDate = ZDateTime.Empty;
			AssertHasError(managerRecord.GSM_EffectiveDateInfo, "An Effective Date must be entered.");

			managerRecord.GSM_EffectiveDate = ZDateTime.Today;
			AssertNoErrors(managerRecord.GSM_EffectiveDateInfo);

			managerRecord.GSM_EffectiveDate = ZDateTime.Today.AddDays(2);
			AssertNoErrors(managerRecord.GSM_EffectiveDateInfo);

			managerRecord.MustBeCurrent = true;
			managerRecord.Validation.ValidateGSM_EffectiveDate();
			AssertEquals("Precondition", false, managerRecord.IsCurrentManager);
			AssertHasError(managerRecord.GSM_EffectiveDateInfo, "This manager must be current. Please change the Effective Dates to make them current.");

			managerRecord.GSM_EffectiveDate = ZDateTime.Empty;
			AssertHasError(managerRecord.GSM_EffectiveDateInfo, "An Effective Date must be entered.");

			managerRecord.GSM_EffectiveDate = ZDateTime.Today;
			AssertEquals("Precondition", true, managerRecord.IsCurrentManager);
			AssertNoErrors(managerRecord.GSM_EffectiveDateInfo);

			managerRecord.GSM_EffectiveDate = ZDateTime.Today.AddDays(-3);
			managerRecord.GSM_EndDate = ZDateTime.Today.AddDays(-2);
			AssertEquals("Precondition", false, managerRecord.IsCurrentManager);
			AssertHasError(managerRecord.GSM_EffectiveDateInfo, "This manager must be current. Please change the Effective Dates to make them current.");
		}

		public void TestGSM_EndDate()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM", new ZDateTime(2019, 01, 01), new ZDateTime(2019, 01, 01));
			managerRecord.RunPreSaveValidation();
			AssertNoErrors(managerRecord.GSM_EndDateInfo);

			managerRecord.GSM_EndDate = new ZDateTime(2019, 01, 02);
			AssertNoErrors(managerRecord.GSM_EndDateInfo);

			managerRecord.GSM_EndDate = new ZDateTime(2018, 12, 31);
			AssertHasError(managerRecord.GSM_EndDateInfo, "End Date cannot be earlier than Effective Date");

			managerRecord.GSM_EndDate = ZDateTime.Empty;
			AssertNoErrors(managerRecord.GSM_EndDateInfo);
		}

		public void TestGSM_GS_Manager()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var otherManager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, staff, "HRM");
			AssertHasError(managerRecord.GSM_GS_ManagerInfo, "A staff member cannot be their own manager.");

			var otherManagerRecord = StaffManagerTestHelper.AddManager(staff, otherManager, "HRM");
			AssertNoError(otherManagerRecord.GSM_GS_ManagerInfo, "A staff member cannot be their own manager.");
		}
	}
}
