using System;
using System.Linq;
using System.Windows.Forms;
using Aga.Controls.Tree;
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
using Enterprise.ZArchitecture.Schema;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(StaffFormForTest))]
	sealed class GlbStaffManagementControlTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestNewButton()
		{
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var button = form.StaffManagementControl.NewToolStripButton;
				AssertNotNull(button);

				button.PerformClick();
				AssertEquals(typeof(GlbStaffManagerForm), ZFormModaliser.ActiveForm.GetType());
			}
		}

		[RequiresSTA]
		public void TestEditButton()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM");
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				form.StaffManagementControl.Tree.SelectionMode = TreeSelectionMode.Multi;
				form.StaffManagementControl.Tree.SelectAllNodes();

				var button = form.StaffManagementControl.EditToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformClick();
				AssertEquals(typeof(GlbStaffManagerForm), ZFormModaliser.ActiveForm.GetType());
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestDetachButton()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM");
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				form.StaffManagementControl.Tree.SelectionMode = TreeSelectionMode.Multi;
				form.StaffManagementControl.Tree.SelectAllNodes();

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);
				AssertContains("Detach button has incorrect label", "Mark as Ended", button.CaptionResourceString.ToString());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
			}
		}

		[RequiresSTA]
		public void TestShouldNotDetachIfMandatoryAndSharedRoleNotAllowed()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "HRM", (NoResString)"Human Resources Manager", true, true, false }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				form.StaffManagementControl.Tree.SelectionMode = TreeSelectionMode.Multi;
				form.StaffManagementControl.Tree.SelectAllNodes();
				AssertEquals(2, form.StaffManagementControl.Tree.SelectedNodes.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);

				button.PerformClick();
				AssertEquals("You cannot detach the only manager in a mandatory role. You can create a new record to supersede this one using the \"New\" button.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldNotDetachIfMandatoryAndOnlyOneManager()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "HRM", (NoResString)"Human Resources Manager", true, true, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				form.StaffManagementControl.Tree.SelectionMode = TreeSelectionMode.Multi;
				form.StaffManagementControl.Tree.SelectAllNodes();
				AssertEquals(2, form.StaffManagementControl.Tree.SelectedNodes.Count);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);

				button.PerformClick();
				AssertEquals("You cannot detach the only manager in a mandatory role. You can create a new record to supersede this one using the \"New\" button.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestShouldDetachIfMandatoryAndManagerNotEffectiveUntilInFuture()
		{
			var roles = new StaffReportingRoleCollection()
			{
				//Code, Description, enabled, isMandatory, isSharedRole
				{ "HRM", (NoResString)"Human Resources Manager", true, true, true }
			};

			SystemDataRegistry.Instance.StaffReportingRoles.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, roles);

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var futureManager = Factory.NewWithValidTestData<GlbStaff>();
			futureManager.GS_FullName = "future";

			var currentManagers = staff.GetCurrentManagers();
			AssertEquals("Precondition", 1, currentManagers.Length);
			currentManagers[0].GSM_EndDate = ZDateTime.Today.AddDays(1);
			var managerRecord = StaffManagerTestHelper.AddManager(staff, futureManager, "HRM", ZDateTime.Today.AddDays(2));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffManagementControl.Tree.AllNodes.FirstOrDefault(x =>
				{
					var managerWrapper = ((GlbStaffManagementTreeNode)x.Tag).BizObj as GlbStaffManagementManagerWrapper;
					return managerWrapper != null && managerWrapper.Manager.IsFutureManager;
				});
				AssertNotNull(managerNode);
				form.StaffManagementControl.Tree.SelectedNode = managerNode;
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);

				button.PerformClick();
				AssertEquals("Future record detach means it should be deleted.", true, managerRecord.IsDeleted);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachDirectManager()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager = Factory.NewWithValidTestData<GlbStaff>();
			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "DRM", new ZDateTime(2019, 04, 04), ZDateTime.Empty);
			AssertNotNull(managerRecord);
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				form.StaffManagementControl.Tree.SelectionMode = TreeSelectionMode.Multi;
				form.StaffManagementControl.Tree.SelectAllNodes();

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
			}
		}

		public void TestHistoryButton()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var previousManager = Factory.NewWithValidTestData<GlbStaff>();
			var currentManager = Factory.NewWithValidTestData<GlbStaff>();

			var noneHrmManagerRecord = StaffManagerTestHelper.AddManager(staff, previousManager, "TRM", new ZDateTime(2018, 01, 01), new ZDateTime(2018, 03, 01));
			var previousManagerRecord = StaffManagerTestHelper.AddManager(staff, previousManager, "HRM", new ZDateTime(2019, 01, 01), new ZDateTime(2019, 03, 01));
			var currentManagerRecord = StaffManagerTestHelper.AddManager(staff, currentManager, "HRM", new ZDateTime(2019, 03, 02));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var button = form.StaffManagementControl.HistoryToolStripButton;
				AssertNotNull(button);
				AssertContains("History button has incorrect label", "View/Edit History", button.CaptionResourceString.ToString());

				button.PerformClick();

				var managerHistoryForm = (GlbStaffManagerHistoryForm)ZFormModaliser.ActiveForm;
				var collection = (GlbStaffManagerCollection)managerHistoryForm.BusinessEntity;
				AssertEquals(3, collection.Count);
			}
		}

		public void TestButtonsShouldBeDisabledWhenSecurityIsDenied()
		{
			var user = GetStaffWithoutPermissions();

			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var previousManager = Factory.NewWithValidTestData<GlbStaff>();
			var currentManager = Factory.NewWithValidTestData<GlbStaff>();

			StaffManagerTestHelper.AddManager(staff, previousManager, "HRM", new ZDateTime(2019, 01, 01), new ZDateTime(2019, 03, 01));
			StaffManagerTestHelper.AddManager(staff, currentManager, "HRM", new ZDateTime(2019, 03, 02));
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (Env.Security.SecurityCachingDisabler)
			using (CurrentUserChanger.SwitchToNewUserTemporarily(user.GS_LoginName))
			{
				using (var form = new StaffFormForTest(model))
				{
					form.Show();
					AssertEquals(false, form.StaffManagementControl.NewToolStripButton.Enabled);
					AssertEquals(false, form.StaffManagementControl.DetachToolStripButton.Enabled);
					AssertEquals(false, form.StaffManagementControl.EditToolStripButton.Enabled);
				}
			}
		}

		public void TestAllowingCycleManagerShouldBeDetached()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_FullName = "staff1";
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_FullName = "cycled manager";

			TestConnection.ExecuteNonQuery($@"DISABLE TRIGGER [dbo].[TG_ManagerTableCyclicalDependencyDetectionTrigger] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var managerRecord1 = StaffManagerTestHelper.AddManager(staff1, staff2, "HRM", new ZDateTime(2019, 01, 01));
			var managerRecord2 = StaffManagerTestHelper.AddManager(staff2, staff1, "HRM", new ZDateTime(2019, 01, 01));
			Factory.Save();

			TestConnection.ExecuteNonQuery($@"ENABLE TRIGGER [dbo].[TG_ManagerTableCyclicalDependencyDetectionTrigger] ON [dbo].[{GlbStaffManagerSchema.Constants.TableName}]");

			var model = new GlbStaffManagementTreeModel(staff1);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				var managerNode = form.StaffManagementControl.Tree.AllNodes.FirstOrDefault(x =>
				{
					var managerWrapper = ((GlbStaffManagementTreeNode)x.Tag).BizObj as GlbStaffManagementManagerWrapper;
					return managerWrapper != null && managerWrapper.Manager.HasCycle();
				});

				AssertNotNull(managerNode);
				form.StaffManagementControl.Tree.SelectedNode = managerNode;

				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);

				button.PerformClick();
				AssertEquals("Cycled record detach means it should be deleted.", true, managerRecord1.IsDeleted);
			}
		}

		[TestDate(2019, 04, 04)]
		[RequiresSTA]
		public void TestDetachManagerWithManagerTypeRemoved()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord = StaffManagerTestHelper.AddManager(staff, manager, "HRM");
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();
				var managerNode = form.StaffManagementControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffManagementControl.Tree.SelectedNode = managerNode;

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);
				AssertContains("Detach button has incorrect label", "Mark as Ended", button.CaptionResourceString.ToString());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				button.PerformClick();

				AssertEquals("End Date should have been set.", new ZDateTime(2019, 04, 04), managerRecord.GSM_EndDate);
			}
		}

		[TestDate(2019, 04, 04)]
		public void TestDetachManagerWithMultipleManagerTypeRemoved()
		{
			StaffManagerTestHelper.SetupBasicRoles();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_FullName = "name";
			var manager1 = Factory.NewWithValidTestData<GlbStaff>();
			var manager2 = Factory.NewWithValidTestData<GlbStaff>();
			var manager3 = Factory.NewWithValidTestData<GlbStaff>();

			var managerRecord1 = StaffManagerTestHelper.AddManager(staff, manager1, "HRM");
			var managerRecord2 = StaffManagerTestHelper.AddManager(staff, manager2, "PRM");
			var managerRecord3 = StaffManagerTestHelper.AddManager(staff, manager3, "TRM");
			Factory.Save();

			var model = new GlbStaffManagementTreeModel(staff);
			model.BuildTree();

			using (var form = new StaffFormForTest(model))
			{
				form.Show();

				SystemDataRegistry.Instance.StaffReportingRoles.Value.RemoveAll();
				var managerNode = form.StaffManagementControl.Tree.AllNodes.FirstOrDefault(x => ((GlbStaffManagementTreeNode)x.Tag).BizObj is GlbStaffManagementManagerWrapper);
				AssertNotNull(managerNode);
				form.StaffManagementControl.Tree.SelectedNode = managerNode;

				var button = form.StaffManagementControl.DetachToolStripButton;
				AssertNotNull(button);
				AssertContains("Detach button has incorrect label", "Mark as Ended", button.CaptionResourceString.ToString());
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				button.PerformClick();
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord1.GSM_EndDate);
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord2.GSM_EndDate);
				AssertEquals("Nothing should have happened.", ZDateTime.Empty, managerRecord3.GSM_EndDate);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);
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
			var payrollManagerRole1 = Factory.NewWithValidTestData<GlbStaffManager>();
			payrollManagerRole1.GSM_ManagerType = "PRM";
			payrollManagerRole1.GSM_EffectiveDate = new ZDateTime(2019, 01, 20);
			payrollManagerRole1.GSM_EndDate = ZDateTime.Now.AddDays(-1);
			payrollManagerRole1.GSM_GS_Staff = staff.PK;
			payrollManagerRole1.GSM_GS_Manager = payrollManagerRole1.PK;

			var payrollManager2 = Factory.NewWithValidTestData<GlbStaff>();
			payrollManager2.GS_FullName = "Dad";
			payrollManager2.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, payrollManager2, "PRM");

			var funManager1 = Factory.NewWithValidTestData<GlbStaff>();
			funManager1.GS_FullName = "Nan";
			funManager1.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, funManager1, "FNM");
			var funManager2 = Factory.NewWithValidTestData<GlbStaff>();
			funManager2.GS_FullName = "Pops";
			funManager2.GS_GB_HomeBranch = branch.PK;
			StaffManagerTestHelper.AddManager(staff, funManager2, "FNM");

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
				Controls.Add(StaffManagementControl);
				BindingSource.SetBindingMember(StaffManagementControl, ".");
				CaptionRenderingEnabled = true;
			}

			public readonly GlbStaffManagementControl StaffManagementControl = new GlbStaffManagementControl();
		}

		#endregion
	}
}
