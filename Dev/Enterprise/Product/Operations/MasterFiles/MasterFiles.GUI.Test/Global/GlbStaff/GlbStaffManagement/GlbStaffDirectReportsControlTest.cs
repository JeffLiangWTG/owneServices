using System;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Types;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(StaffFormForTest))]
	sealed class GlbStaffDirectReportsControlTest : ZFormBasherTest
	{
		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestDetachButton()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachFutureManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 05, 01));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();

				AssertEquals("Record should have been deleted.", true, managerRecord.IsDeleted);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachButtonMandatoryRole()
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
			StaffManagerTestHelper.AddManager(staff, replacementStaff, "HRM");

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();
			AssertEquals("Precondition", 1, replacementStaff.DirectReports.Count);

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;
				form.StaffDirectReportsControl.StaffPopupModuleHelperExposed.SetReplacementStaffCode("RPL");

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Cancel);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("Manager should have been replaced", 2, replacementStaff.DirectReports.Count);

				var report = replacementStaff.DirectReports.FirstOrDefault(x => x.GSM_GS_Staff == directReport.PK);
				AssertNotNull(report);
				AssertEquals("Manager type should match existing record", "HRM", report.GSM_ManagerType);
				AssertEquals("Effective date should be tomorrow", new ZDateTime(2019, 04, 05), report.GSM_EffectiveDate);
				AssertEquals("No end date should be set", ZDateTime.Empty, report.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachButtonNonMandatoryRoleReplaceManager()
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
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;
				form.StaffDirectReportsControl.StaffPopupModuleHelperExposed.SetReplacementStaffCode("RPL");

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				button.PerformClick();

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
		public void TestDetachButtonNonMandatoryRoleReplacingManagerWithThemselvesShouldShowError()
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
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;
				form.StaffDirectReportsControl.StaffPopupModuleHelperExposed.SetReplacementStaffCode("NAM");

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				button.PerformClick();

				AssertEquals("End date should not change", ZDateTime.Empty, managerRecord.GSM_EndDate);
				AssertEquals("Last message should be error message", "You cannot replace the current manager with themselves.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachButtonNonMandatoryRoleReplacingManagerWithExistingManager()
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
			AssertEquals("Precondition", 1, existingManager.DirectReports.Count);

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;
				form.StaffDirectReportsControl.StaffPopupModuleHelperExposed.SetReplacementStaffCode("RPL");

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("Existing manager should not change", 1, existingManager.DirectReports.Count);
				AssertEquals("Existing manager should not change", false, existingManagerRecord.IsDeleted);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachButtonNoSelectionShouldNotDetach()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				AssertEquals("Error message should show", "Please select a manager or managers to remove from the tree", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestDetachButtonRoleSelectedShouldNotDetach()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementRoleWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				AssertEquals("Error message should show", "Please select a manager or managers to remove from the tree", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachDirectManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var directReport = Factory.NewWithValidTestData<GlbStaff>();
			directReport.GS_FullName = "report";
			directReport.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "DRM", new ZDateTime(2019, 01, 01), ZDateTime.Empty);
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("No new managers created.", 1, directReport.Managers.Count);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachAndReplaceDirectManager()
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

			var managerRecord = StaffManagerTestHelper.AddManager(directReport, staff, "DRM", new ZDateTime(2019, 04, 04), ZDateTime.Empty);
			Factory.Save();
			AssertEquals("Pre" +
				"condition", 1, staff.DirectReports.Count);
			AssertEquals("Precondition", 0, replacementStaff.DirectReports.Count);

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;
				form.StaffDirectReportsControl.StaffPopupModuleHelperExposed.SetReplacementStaffCode("RPL");

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
				AssertEquals("Manager should have been replaced", 1, replacementStaff.DirectReports.Count);

				var report = replacementStaff.DirectReports[0];
				AssertEquals("Manager type should match existing record", DefaultStaffReportingRoles.Codes.DirectManager, report.GSM_ManagerType);
				AssertEquals("Effective date should be tomorrow", new ZDateTime(2019, 04, 05), report.GSM_EffectiveDate);
			}
		}

		public void TestButtonsShouldBeDisabledWhenSecurityIsDenied()
		{
			var user = GetStaffWithoutPermissions();

			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(user.GS_LoginName))
			{
				using (var form = new StaffFormForTest(model))
				{
					form.Show();
					AssertEquals(false, form.StaffDirectReportsControl.AttachToolStripButton.Enabled);
					AssertEquals(false, form.StaffDirectReportsControl.DetachToolStripButton.Enabled);
				}
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachManageeWithManagerTypeRemoved()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var managee = Factory.NewWithValidTestData<GlbStaff>();
			managee.GS_FullName = "report";
			managee.GS_Code = "REP";

			var managerRecord = StaffManagerTestHelper.AddManager(managee, staff, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestDetachManageeWithMultipleManagerTypeRemoved()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var managee1 = Factory.NewWithValidTestData<GlbStaff>();
			managee1.GS_FullName = "report1";
			managee1.GS_Code = "RE1";
			var managee2 = Factory.NewWithValidTestData<GlbStaff>();
			managee2.GS_FullName = "report2";
			managee2.GS_Code = "RE2";
			var managee3 = Factory.NewWithValidTestData<GlbStaff>();
			managee3.GS_FullName = "report3";
			managee3.GS_Code = "RE3";

			var managerRecord1 = StaffManagerTestHelper.AddManager(managee1, staff, "HRM", new ZDateTime(2019, 01, 01));
			var managerRecord2 = StaffManagerTestHelper.AddManager(managee2, staff, "PRM", new ZDateTime(2019, 01, 01));
			var managerRecord3 = StaffManagerTestHelper.AddManager(managee3, staff, "TRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff, true);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();

				var managerNode = form.StaffDirectReportsControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffDirectReportsControl.Tree.SelectedNode = managerNode;

				var button = form.StaffDirectReportsControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord1.GSM_EndDate);
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord2.GSM_EndDate);
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord3.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord1.GSM_EndDate);
				AssertEquals("End Date should have been set.", ZDateTime.Empty, managerRecord2.GSM_EndDate);
				AssertEquals("End Date should have been set.", ZDateTime.Empty, managerRecord3.GSM_EndDate);
			}
		}

		#region Implementation

		GlbStaff GetStaffWithoutPermissions()
		{
			var result = Factory.NewWithValidTestData<GlbStaff>();
			result.GS_IsController = false;

			var securityRecord = result.StaffSecurityPermissionsCollection.AddNew();
			securityRecord.GU_SecurityRight = "Maintain";
			securityRecord.GU_SecurityItemIsAllowed = false;

			return result;
		}

		protected override Form GetFormToBashCore()
		{
			var roleCollection = new StaffReportingRoleCollection();
			roleCollection.Add("PRM", (NoResString)"Payroll Manager", true, false, true);
			roleCollection.Add("FNM", (NoResString)"Fun Manager", false, false, true);
			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roleCollection);

			var branch = Factory.NewWithValidTestData<GlbBranch>();
			branch.GB_City = "Sydney";

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "Test Kid";

			var payrollManager1 = Factory.NewWithValidTestData<GlbStaff>();
			payrollManager1.GS_FullName = "Mum";
			payrollManager1.GS_GB_HomeBranch = branch.PK;
			var payrollDirectReport1 = Factory.NewWithValidTestData<GlbStaffManager>();
			payrollDirectReport1.GSM_ManagerType = "PRM";
			payrollDirectReport1.GSM_EffectiveDate = new ZDateTime(2019, 01, 20);
			payrollDirectReport1.GSM_EndDate = ZDateTime.Now.AddDays(-1);
			payrollDirectReport1.GSM_GS_Staff = payrollDirectReport1.PK;
			payrollDirectReport1.GSM_GS_Manager = staff.PK;

			var payrollDirectReport2 = Factory.NewWithValidTestData<GlbStaff>();
			payrollDirectReport2.GS_FullName = "Dad";
			payrollDirectReport2.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(payrollDirectReport2, staff, "PRM");

			var funDirectReport1 = Factory.NewWithValidTestData<GlbStaff>();
			funDirectReport1.GS_FullName = "Nan";
			funDirectReport1.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(funDirectReport1, staff, "FNM");
			var funDirectReport2 = Factory.NewWithValidTestData<GlbStaff>();
			funDirectReport2.GS_FullName = "Pops";
			funDirectReport2.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(funDirectReport2, staff, "FNM");

			var treeModel = new GlbStaffManagementTreeModel(staff);

			return new StaffFormForTest(treeModel);
		}

		public class StaffFormForTest : ZForm
		{
			public StaffFormForTest(GlbStaffManagementTreeModel model)
				: base(model)
			{
			}

			protected override void InitializeComponent()
			{
				base.InitializeComponent();

				Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1024, 768);
				Controls.Add(StaffDirectReportsControl);
				BindingSource.SetBindingMember(StaffDirectReportsControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly GlbStaffDirectReportsControlForTest StaffDirectReportsControl = new GlbStaffDirectReportsControlForTest();
		}

		public class GlbStaffDirectReportsControlForTest : GlbStaffDirectReportsControl
		{
			public GlbStaffPopupModuleHelperForTest StaffPopupModuleHelperExposed => (GlbStaffPopupModuleHelperForTest)StaffPopupModuleHelper;

			protected override GlbStaffPopupModuleHelper StaffPopupModuleHelper
			{
				get
				{
					if (staffPopupModuleHelper == null)
					{
						staffPopupModuleHelper = new GlbStaffPopupModuleHelperForTest();
					}

					return staffPopupModuleHelper;
				}
			}

			GlbStaffPopupModuleHelperForTest staffPopupModuleHelper;
		}

		#endregion
	}
}
