using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using Enterprise.BufferManagement.Integration;
using Enterprise.Core.Forms;
using Enterprise.Environment;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbCapabilityForm))]
	sealed class GlbCapabilityFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestCapabilityMembersGrid_InnerGrid_RowDeleteKeyDownIfRowsDeletingEventArgsCanceled()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				var args = new RowsDeletingEventArgs(new BusinessObject[] { staff1 });
				args.Cancel = true;
				capabilityForm.CapabilityMembersGrid_InnerGrid_RowDeleteKeyDown(null, args);
				AssertNull("The notification should be null", UnitTestUserNotification.Instance.LastMessage.Text);

				args.Cancel = false;
				capabilityForm.CapabilityMembersGrid_InnerGrid_RowDeleteKeyDown(null, args);
				AssertNotNull("The notification should not be null", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCapabilityMembersGrid_UniversalCopyIsNotAllowed()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "WGN";
			capability.G4_Description = "Test Capability";

			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = ResourcesTabIndex;
				Application.DoEvents();

				var grid = form.Controls.Find("CapabilityMembersGrid", true)[0] as ZModuleButtonGrid;
				var menuItem = grid.InnerGrid.ContextMenu.MenuItems.FindByText("Universal Copy", true);

				AssertNull(menuItem);
			}
		}

		[SuppressMessage("Style", "IDE0120:Simplify LINQ expression.", Justification = "This .ToList function has been rewritten.")]
		[SuppressMessage("Style", "IDE0001:Simplify Names", Justification = "This .ToList function has been rewritten.")]
		public void TestReleaseGroupsGridAutoAssignmentTaskAgeIsReadOnly_WhenTaskAutoAssignmentIsNotAllowed()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var group = Factory.NewWithValidTestData<GlbGroup>();

			var pivot = capability.ReleaseGroupPivots.AddNew();
			pivot.GGC_AllowTaskAutoAssignment = false;
			pivot.GGC_AutoAssignTasksAge = new CargoWise.Types.ZDateTime("2013-01-01 00:00:00");
			pivot.GGC_GG_Group = group.PK;

			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = AssignmentTabIndex;
				Application.DoEvents();

				var grid = form.Controls.Find("ReleaseGroupsGrid", true)[0] as ZGrid;
				AssertNotNull(grid);

				var ageColumn = grid.Columns.IndexOf(c => c.ColumnName == "GGC_AutoAssignTasksAge");
				grid.CurrentCell = new DataGridCell(0, ageColumn);

				var cell = grid.Controls.ToList<Control>().First(ctrl => ctrl.Name.Equals("GGC_AutoAssignTasksAge")) as DataGridTextBox;
				AssertEquals(true, cell.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestReleaseGroupsGridIsReplacedWithLabel_WhenCapacityScopeIsGlobal()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";
			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tabControl.SelectedIndex = AssignmentTabIndex;
				Application.DoEvents();

				var groupBox = form.Controls.Find("ReleaseGroupSpecificConfigurationGroupBox", true)[0] as ZGroupBox;
				AssertNotNull(groupBox);
				AssertEquals(true, groupBox.Visible);

				var label = form.Controls.Find("ReleaseGroupsInaccessibleLabel", true)[0] as ZLabel;
				AssertNotNull(label);
				AssertEquals(false, label.Visible);

				capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
				tabControl.SelectedIndex = DetailsTabIndex; // let's switch to other tab and back
				Application.DoEvents();
				tabControl.SelectedIndex = AssignmentTabIndex;
				Application.DoEvents();

				AssertEquals(false, groupBox.Visible);
				AssertEquals(true, label.Visible);

				capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
				tabControl.SelectedIndex = DetailsTabIndex; // let's switch to other tab and back
				Application.DoEvents();
				tabControl.SelectedIndex = AssignmentTabIndex;
				Application.DoEvents();

				AssertEquals(true, groupBox.Visible);
				AssertEquals(false, label.Visible);
			}

			capability.G4_CapacityScope = GlbCapabilityScopeList.Codes.GlobalScope;
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tabControl = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tabControl.SelectedIndex = AssignmentTabIndex;
				Application.DoEvents();

				var groupBox = form.Controls.Find("ReleaseGroupSpecificConfigurationGroupBox", true)[0] as ZGroupBox;
				AssertNotNull(groupBox);
				AssertEquals(false, groupBox.Visible);

				var label = form.Controls.Find("ReleaseGroupsInaccessibleLabel", true)[0] as ZLabel;
				AssertNotNull(label);
				AssertEquals(true, label.Visible);
			}
		}

		public void TestDetachResourceFromCapability()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";
			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = ResourcesTabIndex;
				Application.DoEvents();

				var grid = form.Controls.Find("CapabilityMembersGrid", true)[0] as ZModuleButtonGrid;
				var toolStrip = grid.Controls.Find("toolStrip", true)[0] as ZToolStrip;
				toolStrip.Items.Find(ZModuleButtonGrid.Buttons.Detach, true)[0].PerformClick();
				Application.DoEvents();

				AssertEquals(ContinueWithSave.Yes, form.FireSaveButton());
				Application.DoEvents();
			}
		}

		public void TestExistenceOfWorkStatusColumn()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";
			capability.ResourcePivot = Factory.NewWithValidTestData<GlbResourceCapabilityPivot>();
			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = ResourcesTabIndex;
				Application.DoEvents();

				var grid = form.Controls.Find("CapabilityMembersGrid", true)[0] as ZModuleButtonGrid;
				var columns = grid.InnerGrid.Columns;
				AssertEquals("CapacityMembersGrid does not contain the WorkStatus column", true, columns.Contains("WorkStatus"));
			}
		}

		[RequiresSTA]
		public void TestClickGridElements_ShouldOpenStaffForm()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";

			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = ResourcesTabIndex;
				Application.DoEvents();

				var grid = form.Controls.Find("CapabilityMembersGrid", true)[0] as ZModuleButtonGrid;
				grid.InnerGrid.PerformMouseDoubleClickForTest(0);

				using (var staffForm = Application.OpenForms.OfType<GlbStaffForm>().SingleOrDefault())
				{
					var staffFormBizo = (GlbStaff)staffForm.CurrentDataItem;
					AssertNotNull("Double clicking a grid element should open the corresponding staff form!", staffForm);
					CombineAssertions("GlbStaff has no .Equals override method for comparing two staff bizos, so we do this", () =>
					{
						AssertEquals("Code is equal", staff.GS_Code, staffFormBizo.GS_Code);
						AssertEquals("FullName is equal", staff.GS_FullName, staffFormBizo.GS_FullName);
						AssertEquals("Password is equal but don't look at it", staff.GS_PasswordHash, staffFormBizo.GS_PasswordHash);
					});
				}
			}
		}

		[RequiresSTA]
		public void TestHomeBranchAndHomeDepartmentColumns()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "QWE";
			capability.G4_Description = "Qwerty Keyboard Typing";

			var staff = Factory.New<GlbStaff>();
			staff.GS_Code = "DVK";
			staff.GS_FullName = "Antonin Dvorak";

			var branch = Factory.New<GlbBranch>();
			branch.GB_Code = "PRG";
			branch.GB_BranchName = "Prague";
			branch.GB_GC = Environment.Env.CurrentCompanyPK;

			var department = Factory.New<GlbDepartment>();
			department.GE_Code = "SAN";
			department.GE_Desc = "Sanitation";

			staff.GS_GB_HomeBranch = branch.PK;
			staff.GS_GE_HomeDepartment = department.PK;
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				form.Show();
				Application.DoEvents();

				var tab = form.Controls.Find("MainTabControl", true)[0] as ZTemplateTabControl;
				tab.SelectedIndex = ResourcesTabIndex;
				Application.DoEvents();

				var grid = form.Controls.Find("CapabilityMembersGrid", true)[0] as ZModuleButtonGrid;
				var columnHeaders = grid.InnerGrid.Columns.Select(c => c.ColumnStyle.HeaderText);

				var homeBranchIndex = grid.InnerGrid.Columns.IndexOf(column => column.ColumnName == "HomeBranch+GB_Code");
				var homeDepartmentIndex = grid.InnerGrid.Columns.IndexOf(column => column.ColumnName == "HomeDepartment+GE_Code");

				CombineAssertions(() =>
				{
					AssertEquals(true, columnHeaders.Contains("Home Branch"));
					AssertEquals(true, columnHeaders.Contains("Home Department"));
					AssertEquals("PRG", grid.InnerGrid[0, homeBranchIndex]);
					AssertEquals("SAN", grid.InnerGrid[0, homeDepartmentIndex]);
				});
			}
		}

		public void TestNewReleaseGateColumns_OnlyAppearWhenDisplayResponsiveReleaseGateUiSettings_IsEnabled()
		{
			var capability = Factory.New<GlbCapability>();
			capability.G4_Code = "TST";
			capability.G4_Description = "Test Capability";
			var staff = Factory.New<GlbStaff>();
			staff.Capabilities.Add(capability);
			Factory.Save();

			using (var form = new GlbCapabilityForm(capability))
			{
				var releaseGroupsGrid = (ZGrid)form.Controls.Find("ReleaseGroupsGrid", searchAllChildren: true).First();
				var columns = releaseGroupsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName);
				Assert(!columns.Contains("GGC_CapabilityStartableWorkflowLimit"));
			}

			ObjectFactory.Get<IBMSRegistry>().DisplayResponsiveReleaseGateUiSettings = true;

			using (var form = new GlbCapabilityForm(capability))
			{
				var releaseGroupsGrid = (ZGrid)form.Controls.Find("ReleaseGroupsGrid", searchAllChildren: true).First();
				var columns = releaseGroupsGrid.ColumnStyles.Cast<ZGridColumnInfo>().Select(c => c.ColumnName);
				Assert(columns.Contains("GGC_CapabilityStartableWorkflowLimit"));
			}
		}

		#region Detaching Staff From Capability Tests

		[RequiresSTA]
		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			DetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast(allowed: true, "Do you still want to remove the staff members from this capability?", MessageBoxButtons.YesNo);
		}

		[RequiresSTA]
		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast_AndOperationIsNotAllowed()
		{
			DetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast(allowed: false, "You do not have the relevant permission to make this change.", MessageBoxButtons.OK);
		}

		void DetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast(bool allowed, string message, MessageBoxButtons buttons)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			// see corresponding test in MasterFiles (TestGetLastStaffWithGRPCapabilityPerGroup)
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = false;
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_IsSecurityEnabled = false;
			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Code = "G4";
			group4.GG_IsSecurityEnabled = false;
			var group5 = Factory.NewWithValidTestData<GlbGroup>();
			group5.GG_Code = "G5";
			group5.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group3);
			staff1.Groups.Add(group4);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group2);
			staff2.Groups.Add(group3);
			staff2.Groups.Add(group5);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability1);
			staff3.Groups.Add(group4);
			staff3.Groups.Add(group5);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.CapabilityMembersGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1, staff2 }));

				AssertEquals(@"- Last member(s) with this capability in G3 group: S1, S2.
- Last member(s) with this capability in G1 group: S1.
- Last member(s) with this capability in G2 group: S2.

Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. " + message, capabilityForm.CapabilityMembersGrid.DetachMessage.Caption);

				AssertEquals(capabilityForm.MessageBoxButtons, buttons);
			}
		}

		[RequiresSTA]
		public void TestDetachingStaff_ShouldShowDefaultMessage_WhenStaffIsNotLast()
		{
			DetachingStaff_ShouldShowDefaultMessage_WhenStaffIsNotLast(allowed: true);
		}

		public void TestDetachingStaff_ShouldShowDefaultMessage_WhenStaffIsNotLast_AndOperationIsNotAllowed()
		{
			DetachingStaff_ShouldShowDefaultMessage_WhenStaffIsNotLast(allowed: false);
		}

		void DetachingStaff_ShouldShowDefaultMessage_WhenStaffIsNotLast(bool allowed)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.CapabilityMembersGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1 }));

				AssertEquals("Are you sure you want to remove the selected staff members from this Capability?", capabilityForm.CapabilityMembersGrid.DetachMessage.Caption);
				AssertEquals(capabilityForm.MessageBoxButtons, MessageBoxButtons.YesNo);
			}
		}

		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast_OnlyForNonSecurityGroups()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = true;
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_IsSecurityEnabled = false;
			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Code = "G4";
			group4.GG_IsSecurityEnabled = false;
			var group5 = Factory.NewWithValidTestData<GlbGroup>();
			group5.GG_Code = "G5";
			group5.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group3);
			staff1.Groups.Add(group4);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group2);
			staff2.Groups.Add(group3);
			staff2.Groups.Add(group5);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability1);
			staff3.Groups.Add(group4);
			staff3.Groups.Add(group5);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.CapabilityMembersGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1, staff2 }));

				AssertEquals(@"- Last member(s) with this capability in G3 group: S1, S2.

Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this capability?", capabilityForm.CapabilityMembersGrid.DetachMessage.Caption);
			}
		}

		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast_ForAllGroups()
		{
			WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;
			var group2 = Factory.NewWithValidTestData<GlbGroup>();
			group2.GG_Code = "G2";
			group2.GG_IsSecurityEnabled = true;
			var group3 = Factory.NewWithValidTestData<GlbGroup>();
			group3.GG_Code = "G3";
			group3.GG_IsSecurityEnabled = false;
			var group4 = Factory.NewWithValidTestData<GlbGroup>();
			group4.GG_Code = "G4";
			group4.GG_IsSecurityEnabled = false;
			var group5 = Factory.NewWithValidTestData<GlbGroup>();
			group5.GG_Code = "G5";
			group5.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);
			staff1.Groups.Add(group3);
			staff1.Groups.Add(group4);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group2);
			staff2.Groups.Add(group3);
			staff2.Groups.Add(group5);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability1);
			staff3.Groups.Add(group4);
			staff3.Groups.Add(group5);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.CapabilityMembersGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1, staff2 }));

				AssertEquals(@"- Last member(s) with this capability in G3 group: S1, S2.
- Last member(s) with this capability in G1 group: S1.
- Last member(s) with this capability in G2 group: S2.

Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this capability?", capabilityForm.CapabilityMembersGrid.DetachMessage.Caption);
			}
		}

		public void TestDeleteKey_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.TopLevelTabControl.SelectedTab = capabilityForm.MembersTabPage;
				capabilityForm.CapabilityMembersGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(capabilityForm.CapabilityMembersGrid.InnerGrid, Keys.Delete);
				Application.DoEvents();

				AssertEquals(@"- Last member(s) with this capability in G1 group: S1, S2.

Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this capability?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestDeleteKey_ShouldNotRemoveStaff_WhenResponseIsNo()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.TopLevelTabControl.SelectedTab = capabilityForm.MembersTabPage;
				capabilityForm.CapabilityMembersGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(capabilityForm.CapabilityMembersGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Application.DoEvents();

				AssertEquals("Staff should not be detached", 2, group1.Staff.Count);
			}
		}

		public void TestDeleteKey_ShouldRemoveStaff_WhenResponseIsYes()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.TopLevelTabControl.SelectedTab = capabilityForm.MembersTabPage;
				capabilityForm.CapabilityMembersGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(capabilityForm.CapabilityMembersGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();

				AssertEquals("Staff should be detached", 0, capability1.ResourcesWithCapability.Count);
			}
		}

		[RequiresSTA]
		public void TestDeleteMenuItemClick_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.TopLevelTabControl.SelectedTab = capabilityForm.MembersTabPage;
				capabilityForm.CapabilityMembersGrid.InnerGrid.SelectAllElements();
				capabilityForm.CapabilityMembersGrid.InnerGrid.DeleteMenuItem.PerformClick();

				AssertEquals(@"- Last member(s) with this capability in G1 group: S1.

Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this capability?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestDetachingStaff_ShouldShowDefaultMessage_ThenWarningMessage_WhenDeleteingRowsOneAtATime()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability1);
			staff2.Groups.Add(group1);

			Factory.Save();

			using (var capabilityForm = new GlbCapabilityForm(capability1))
			{
				capabilityForm.Show();
				capabilityForm.TopLevelTabControl.SelectedTab = capabilityForm.MembersTabPage;

				capabilityForm.CapabilityMembersGrid.InnerGrid.SelectSingleElementByPK(staff1.PK);
				KeySender.PostKeyDown(capabilityForm.CapabilityMembersGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				AssertEquals("Are you sure you want to remove the selected staff members from this Capability?", UnitTestUserNotification.Instance.LastMessage.Text);

				capabilityForm.CapabilityMembersGrid.InnerGrid.SelectSingleElementByPK(staff2.PK);
				KeySender.PostKeyDown(capabilityForm.CapabilityMembersGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				AssertEquals(@"- Last member(s) with this capability in G1 group: S2.

Removing the staff members from this capability will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this capability?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		const int DetailsTabIndex = 0;
		const int AssignmentTabIndex = 1;
		const int ResourcesTabIndex = 2;

		protected override Form GetFormToBashCore()
		{
			return new GlbCapabilityForm(Factory.New<GlbCapability>());
		}
	}
}
