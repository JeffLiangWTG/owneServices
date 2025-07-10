using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using NUnit.Framework;

namespace Enterprise.MasterFiles.Business.Testing
{
	[TestedType(typeof(AddDirectReportsBizO))]
	sealed class AddDirectReportsBiZoTest : NonPersistentBusinessObjectTestCase, IObsoleteValidation
	{
		public void TestValidateManagerType()
		{
			StaffManagerTestHelper.SetupBasicRoles();

			var staff = Factory.NewWithValidTestData<GlbStaff>();

			var addDirectReportsBiZo = new AddDirectReportsBizO(staff, "AAA");
			AssertHasError("This is not a valid manager type", addDirectReportsBiZo.ManagerTypeInfo, "Enter a valid selection.");
			addDirectReportsBiZo.ManagerType = "DRM";
			AssertNoErrors("This is a valid manager type", addDirectReportsBiZo.ManagerTypeInfo);
			addDirectReportsBiZo.ManagerType = "BBB";
			AssertHasError("This is not a valid manager type", addDirectReportsBiZo.ManagerTypeInfo, "Enter a valid selection.");
			addDirectReportsBiZo.ManagerType = string.Empty;
			AssertHasError("This is not a valid manager type", addDirectReportsBiZo.ManagerTypeInfo, "Please enter a value.");
		}

		public void TestAddDirectReports()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(staff, "PRM");

			AssertEquals(true, addDirectReportsBiZo.AddNewManagerForStaff(staff));
			AssertEquals("The staff member should have been added as their own manager", 1, staff.DirectReports.Count);
			AssertEquals("The staff member should have been added as their own manager", staff, staff.DirectReports[0].Staff);
		}

		public void TestAddDirectReportsButtonSupersede()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			var existingReportManagerRecord = StaffManagerTestHelper.AddManager(existingReport, existingReport, "HRM", new ZDateTime(2019, 04, 04));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "HRM")
			{
				EffectiveDate = new ZDateTime(2019, 06, 13)
			};

			AssertEquals(true, addDirectReportsBiZo.AddNewManagerForStaff(existingReport, true));
			AssertEquals("ExistingReport's manager should have been superseded", 1, manager.DirectReports.Count);
			AssertEquals("ExistingReport's manager should have been superseded", false, existingReportManagerRecord.IsDeleted);
			AssertEquals("ExistingReport's manager should have been superseded", addDirectReportsBiZo.EffectiveDate.AddDays(-1), existingReportManagerRecord.GSM_EndDate);
		}

		[TestDate(2019, 05, 05)]
		public void TestAddDirectReportsButtonExistingManagerEncompassed()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			existingReport.GS_FullName = "Toad Stool";
			var existingReport2 = Factory.NewWithValidTestData<GlbStaff>();
			existingReport2.GS_FullName = "Toast Duel";
			StaffManagerTestHelper.AddManager(existingReport, existingReport, "PRM", new ZDateTime(2019, 03, 04));
			StaffManagerTestHelper.AddManager(existingReport2, existingReport2, "PRM", new ZDateTime(2019, 03, 04));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "PRM")
			{
				EffectiveDate = new ZDateTime(2019, 01, 01)
			};

			AssertEquals(false, addDirectReportsBiZo.AddNewManagerForStaff(existingReport, true));
			AssertEquals(false, addDirectReportsBiZo.AddNewManagerForStaff(existingReport2, true));
			AssertEquals("ExistingReports' managers should not have been superseded", 0, manager.DirectReports.Count);
		}

		[TestDate(2019, 05, 05)]
		public void TestAddDirectReportsButtonFutureManagerEncompassed()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			var existingReportManagerRecord = StaffManagerTestHelper.AddManager(existingReport, existingReport, "PRM", new ZDateTime(2019, 07, 07));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "PRM")
			{
				EffectiveDate = new ZDateTime(2019, 01, 01)
			};

			AssertEquals(true, addDirectReportsBiZo.AddNewManagerForStaff(existingReport, true));
			AssertEquals("ExistingReport's manager should have been deleted", 1, manager.DirectReports.Count);
			AssertEquals("ExistingReport's manager should have been deleted", true, existingReportManagerRecord.IsDeleted);
		}

		[TestDate(2019, 06, 13)]
		public void TestAddDirectReportsButtonExistingManagerOverlap()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var existingReport = Factory.NewWithValidTestData<GlbStaff>();
			var existingReportManagerRecord = StaffManagerTestHelper.AddManager(existingReport, existingReport, "PRM", new ZDateTime(2019, 04, 04));
			Factory.Save();

			var addDirectReportsBiZo = new AddDirectReportsBizO(manager, "PRM")
			{
				EffectiveDate = new ZDateTime(2019, 06, 13)
			};

			AssertEquals(true, addDirectReportsBiZo.AddNewManagerForStaff(existingReport, true));
			AssertEquals("ExistingReport's manager should have been superseded", 1, manager.DirectReports.Count);
			AssertEquals("ExistingReport's manager should have been superseded", false, existingReportManagerRecord.IsDeleted);
			AssertEquals("ExistingReport's manager should have been superseded", addDirectReportsBiZo.EffectiveDate.AddDays(-1), existingReportManagerRecord.GSM_EndDate);
		}

		#region Implementation

		protected override BusinessObject GetNewBusinessObject()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			return new AddDirectReportsBizO(staff, "HRM");
		}

		#endregion
	}
}
