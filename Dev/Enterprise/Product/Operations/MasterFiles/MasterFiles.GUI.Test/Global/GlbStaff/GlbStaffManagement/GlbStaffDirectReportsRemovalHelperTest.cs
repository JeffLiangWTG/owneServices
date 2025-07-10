using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.EntityFramework.Testing;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	sealed class GlbStaffDirectReportsRemovalHelperTest : TestCaseWithFactory
	{
		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestTransferDirectReports()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources Manager", true, true, true }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var replacementStaff = Factory.NewWithValidTestData<GlbStaff>();
			replacementStaff.GS_FullName = "replacement";
			replacementStaff.GS_Code = "RPL";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();
			AssertEquals("Precondition", 1, staff.DirectReports.Count);
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			var staffPopupModuleHelper = new GlbStaffPopupModuleHelperForTest();
			staffPopupModuleHelper.SetReplacementStaffCode("RPL");

			using (var form = new ZForm(staff))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				var result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, staffPopupModuleHelper, form);

				var today = new ZDateTime(2019, 04, 04);

				AssertEquals("Transfer should succeed", true, result);
				AssertEquals("End Date should have been set.", today, managerRecord.GSM_EndDate);
				AssertEquals("Manager should have been replaced", 1, replacementStaff.DirectReports.Count);

				var newReport = replacementStaff.DirectReports.Single();
				AssertEquals("Manager type should match existing record", "HRM", newReport.GSM_ManagerType);
				AssertEquals("Effective date should be tomorrow", new ZDateTime(2019, 04, 05), newReport.GSM_EffectiveDate);
				AssertEquals("Direct report should remain the same", directReport.PK, newReport.GSM_GS_Staff);
				AssertEquals("No end date should be set", ZDateTime.Empty, newReport.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestRemoveSelfManagedStaffInMandatoryRole()
		{
			var roles = new StaffReportingRoleCollection()
				{
					//Code, Description, enabled, isMandatory, isSharedRole
					{ "HRM", (NoResString)"Human Resources Manager", true, true, true }
				};
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";

			var managerLink = staff.Managers.AddNew();
			managerLink.GSM_GS_Manager = staff.PK;
			managerLink.GSM_EffectiveDate = ZDateTime.Now.AddDays(-10);
			managerLink.GSM_ManagerType = "HRM";

			Factory.Save();
			AssertEquals("Precondition", 1, staff.DirectReports.Count);
			var managerRecord = staff.DirectReports[0];

			using (var form = new ZForm(staff))
			{
				form.Show();

				var result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, new GlbStaffPopupModuleHelperForTest(), form);
				AssertEquals("Remove should succeed", true, result);
				AssertEquals("End Date should have been set to today.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestRemoveStaffWithDirectReportsInNonMandatoryRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var replacementStaff = Factory.NewWithValidTestData<GlbStaff>();
			replacementStaff.GS_FullName = "replacement";
			replacementStaff.GS_Code = "RPL";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			var oldManagerRecord = StaffManagerTestHelper.AddManager(replacementStaff, staff, "HRM", new ZDateTime(2018, 01, 11), new ZDateTime(2018, 12, 03));
			var futureManagerRecord = StaffManagerTestHelper.AddManager(replacementStaff, staff, "HRM", new ZDateTime(2019, 04, 05));
			Factory.Save();
			AssertEquals("Precondition", 3, staff.DirectReports.Count);
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			using (var form = new ZForm(staff))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				var result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, new GlbStaffPopupModuleHelperForTest(), form);
				AssertEquals("Remove should succeed", true, result);
				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("Old End Date should not have changed.", new ZDateTime(2018, 12, 03), oldManagerRecord.GSM_EndDate);
				AssertEquals("Future manager should be deleted.", true, futureManagerRecord.IsDeleted);
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestTransferDirectReportsInNonMandatoryRole()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var replacementStaff = Factory.NewWithValidTestData<GlbStaff>();
			replacementStaff.GS_FullName = "replacement";
			replacementStaff.GS_Code = "RPL";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();
			AssertEquals("Precondition", 1, staff.DirectReports.Count);
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			var staffPopupModuleHelper = new GlbStaffPopupModuleHelperForTest();
			staffPopupModuleHelper.SetReplacementStaffCode("RPL");

			using (var form = new ZForm(staff))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, staffPopupModuleHelper, form);
				AssertEquals("Transfer should succeed", true, result);
				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("Manager should have been replaced", 1, replacementStaff.DirectReports.Count);

				var report = replacementStaff.DirectReports[0];
				AssertEquals("Manager type should match existing record", "HRM", report.GSM_ManagerType);
				AssertEquals("Effective date should be tomorrow", new ZDateTime(2019, 04, 05), report.GSM_EffectiveDate);
				AssertEquals("Direct report should remain the same", directReport.PK, report.GSM_GS_Staff);
				AssertEquals("No end date should be set", ZDateTime.Empty, report.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestTransferDirectReportsToSameStaffShouldShowError()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			staff.GS_Code = "NAM";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var replacementStaff = Factory.NewWithValidTestData<GlbStaff>();
			replacementStaff.GS_FullName = "replacement";
			replacementStaff.GS_Code = "RPL";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();
			AssertEquals("Precondition", 1, staff.DirectReports.Count);
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			var staffPopupModuleHelper = new GlbStaffPopupModuleHelperForTest();
			staffPopupModuleHelper.SetReplacementStaffCode("NAM");

			using (var form = new ZForm(staff))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, staffPopupModuleHelper, form);
				AssertEquals("Transfer should fail", false, result);
				AssertEquals("End date should not change", ZDateTime.Empty, managerRecord.GSM_EndDate);
				AssertEquals("Last message should be error message", "You cannot replace the current manager with themselves.", UnitTestUserNotification.Instance.LastMessage.Text);

				staffPopupModuleHelper.SetReplacementStaffCode("RPL");
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, staffPopupModuleHelper, form);
				AssertEquals("Transfer should succeed", true, result);
				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertNotEquals("Last message should not be error", "You cannot replace the current manager with themselves.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestTransferDirectReportsToExistingManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			staff.GS_Code = "NAM";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";
			var existingManager = Factory.NewWithValidTestData<GlbStaff>();
			existingManager.GS_FullName = "replacement";
			existingManager.GS_Code = "RPL";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			var existingManagerRecord = StaffManagerTestHelper.AddManager(directReport, existingManager, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();
			AssertEquals("Precondition", 1, staff.DirectReports.Count);
			AssertEquals("Precondition", 1, existingManager.DirectReports.Count);

			var staffPopupModuleHelper = new GlbStaffPopupModuleHelperForTest();
			staffPopupModuleHelper.SetReplacementStaffCode("RPL");

			using (var form = new ZForm(staff))
			{
				form.Show();

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				var result = GlbStaffDirectReportsRemovalHelper.TransferOrRemoveDirectReports(staff, staffPopupModuleHelper, form);
				AssertEquals("Transfer should succeed", true, result);
				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("Existing manager should not change", 1, existingManager.DirectReports.Count);
				AssertEquals("Existing manager should not change", false, existingManagerRecord.IsDeleted);
			}
		}
	}
}
