using CargoWise.Types;
using Enterprise.ZArchitecture.Business.Testing;
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(GlbStaffManager))]
	sealed class GlbStaffManagerTest : EnterpriseBusinessObjectTestCase
	{
		public void TestGSM_ManagerTypeReadOnly()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord = Factory.NewWithValidTestData<GlbStaffManager>();

			AssertEquals(false, managerRecord.GSM_ManagerTypeInfo.ReadOnly);
			StaffManagerTestHelper.AddManager(staff, staff, "HRM");
			managerRecord.GSM_GS_Staff = staff.PK;
			managerRecord.GSM_GS_Manager = staff.PK;
			managerRecord.GSM_EffectiveDate = ZDate.Today;
			Factory.Save();

			AssertEquals(true, managerRecord.GSM_ManagerTypeInfo.ReadOnly);
		}

		public void TestGetOtherManagersWithOverlappingPeriodsWhenNoStaffShouldNotThrowException()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var managerRecord = Factory.NewWithValidTestData<GlbStaffManager>();
			AssertNoExceptionThrown(() => managerRecord.GetOtherManagersWithOverlappingPeriods());
		}

		[TestDate(2019, 04, 04)]
		public void TestCanAddRoleForStaff()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();

			var manager1HRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01));
			AssertEquals(true, manager1HRMRecord.CanAddRoleForStaff());
			var manager1PRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "PRM", new ZDateTime(2019, 01, 01));
			AssertEquals(true, manager1PRMRecord.CanAddRoleForStaff());
			Factory.Save();

			var manager2HRMRecord = StaffManagerTestHelper.AddManager(staff, manager2, "HRM", ZDate.Today);
			AssertEquals(true, manager2HRMRecord.CanAddRoleForStaff());

			var manager2PRMRecord = StaffManagerTestHelper.AddManager(staff, manager2, "PRM", ZDate.Today);
			AssertEquals(false, manager2PRMRecord.CanAddRoleForStaff());

			manager1PRMRecord.GSM_EndDate = new ZDateTime(2019, 04, 03);
			AssertEquals(true, manager2PRMRecord.CanAddRoleForStaff());

			manager1PRMRecord.GSM_EndDate = new ZDateTime(2019, 04, 04);
			AssertEquals(false, manager2PRMRecord.CanAddRoleForStaff());

			manager2PRMRecord.GSM_EffectiveDate = new ZDateTime(2018, 04, 04);
			manager2PRMRecord.GSM_EndDate = new ZDateTime(2018, 12, 31);
			AssertEquals(true, manager2PRMRecord.CanAddRoleForStaff());

			manager2PRMRecord.GSM_EndDate = new ZDateTime(2019, 01, 01);
			AssertEquals(false, manager2PRMRecord.CanAddRoleForStaff());
		}

		public void TestSelfManaged()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();

			var manager1HRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01));
			AssertEquals(false, manager1HRMRecord.SelfManaged);

			manager1HRMRecord.GSM_GS_Manager = staff.PK;
			AssertEquals(true, manager1HRMRecord.SelfManaged);
		}

		public void TestGSM_EffectiveDate()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();

			var manager1DRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "DRM", new ZDateTime(2019, 03, 03), ZDateTime.Empty);
			var manager1HRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01));
			AssertNotNull(manager1DRMRecord);

			AssertEquals(new ZDateTime(2019, 01, 01), manager1HRMRecord.GSM_EffectiveDate);

			manager1DRMRecord.GSM_EffectiveDate = new ZDateTime(2019, 05, 05);
			manager1HRMRecord.GSM_EffectiveDate = new ZDateTime(2019, 06, 06);

			AssertEquals(new ZDateTime(2019, 05, 05), manager1DRMRecord.GSM_EffectiveDate);
			AssertEquals(new ZDateTime(2019, 06, 06), manager1HRMRecord.GSM_EffectiveDate);
		}

		public void TestGSM_EndDate()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();

			var manager1DRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "DRM", new ZDateTime(2019, 03, 03), new ZDateTime(2019, 04, 04));
			var manager1HRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01), new ZDateTime(2019, 02, 02));
			AssertNotNull(manager1DRMRecord);

			AssertEquals(new ZDateTime(2019, 02, 02), manager1HRMRecord.GSM_EndDate);

			manager1DRMRecord.GSM_EndDate = new ZDateTime(2019, 05, 05);
			manager1HRMRecord.GSM_EndDate = new ZDateTime(2019, 06, 06);

			AssertEquals(new ZDateTime(2019, 05, 05), manager1DRMRecord.GSM_EndDate);
			AssertEquals(new ZDateTime(2019, 06, 06), manager1HRMRecord.GSM_EndDate);
		}

		[TestDate(2019, 03, 03)]
		public void TestIsCurrentManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();

			var manager1DRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "DRM", new ZDateTime(2019, 03, 03), new ZDateTime(2019, 04, 06));
			var manager1HRMRecord = StaffManagerTestHelper.AddManager(staff, manager1, "HRM", new ZDateTime(2019, 01, 01), new ZDateTime(2019, 02, 02));
			AssertNotNull(manager1DRMRecord);

			AssertEquals(true, manager1DRMRecord.IsCurrentManager);
			AssertEquals(false, manager1HRMRecord.IsCurrentManager);

			manager1DRMRecord.GSM_EffectiveDate = new ZDateTime(2019, 03, 05);
			manager1HRMRecord.GSM_EndDate = new ZDateTime(2019, 06, 06);

			AssertEquals(false, manager1DRMRecord.IsCurrentManager);
			AssertEquals(true, manager1HRMRecord.IsCurrentManager);
		}

		[TestDate(2019, 02, 03)]
		public void TestHasCycle()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[TG_ManagerTableCyclicalDependencyDetectionTrigger] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var cycleRecord1 = StaffManagerTestHelper.AddManager(staff1, staff2, "TRM", new ZDateTime(2019, 03, 03), new ZDateTime(2019, 04, 06));
			var cycleRecord2 = StaffManagerTestHelper.AddManager(staff2, staff1, "TRM", new ZDateTime(2019, 03, 03), new ZDateTime(2019, 04, 06));

			Factory.Save();
			TestConnection.ExecuteNonQuery($@"ENABLE TRIGGER [dbo].[TG_ManagerTableCyclicalDependencyDetectionTrigger] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			AssertEquals(true, cycleRecord1.HasCycle());
			AssertEquals(true, cycleRecord2.HasCycle());
		}

		[TestDate(2019, 02, 03)]
		public void TestHasCycleWithEmptyEndDate()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();

			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[TG_ManagerTableCyclicalDependencyDetectionTrigger] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var cycleRecord1 = StaffManagerTestHelper.AddManager(staff1, staff2, "TRM", new ZDateTime(2019, 03, 03), ZDateTime.Empty);
			var cycleRecord2 = StaffManagerTestHelper.AddManager(staff2, staff1, "TRM", new ZDateTime(2019, 03, 03), ZDateTime.Empty);
			Factory.Save();

			TestConnection.ExecuteNonQuery($@"ENABLE TRIGGER [dbo].[TG_ManagerTableCyclicalDependencyDetectionTrigger] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			AssertEquals(true, cycleRecord1.HasCycle());
			AssertEquals(true, cycleRecord2.HasCycle());
		}
	}
}
