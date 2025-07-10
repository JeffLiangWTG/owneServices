using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.ComponentModel;
using CargoWise.EntityFramework;
using CargoWise.Glow.CW1.ApplicationCheckpoints.DevIntegration;
using CargoWise.Integration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using Enterprise.BufferManagement.Integration;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.CustomValues;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Core.Testing;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.GUI.Testing;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;
using Moq;
using NUnit.Framework;

namespace Enterprise.MasterFiles.GUI.Testing
{
	[TestedType(typeof(GlbGroupForm))]
	sealed class GlbGroupFormTest : ZFormBasherTest
	{
		[RequiresSTA]
		public void TestButtonsReadonly()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_ExternalId = "";

			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals(true, testForm.MembersModuleButtonGrid.AttachButton.Enabled);
				AssertEquals(true, testForm.MembersModuleButtonGrid.DetachButton.Enabled);
			}

			group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_ExternalId = "123";

			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals(false, testForm.MembersModuleButtonGrid.AttachButton.Enabled);
				AssertEquals(false, testForm.MembersModuleButtonGrid.DetachButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestCannotAddToScimGroup()
		{
			SystemDataRegistry.Instance.ScimAllowLocalEditing.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, true);
			var mappings = new StaffColumnToGroupDescriptionScimMappingCollection();
			mappings.Add(new StaffColumnToGroupDescriptionScimMapping() { StaffColumnName = "GS_IsController", GroupDescriptionMapping = "is controller" });
			SystemDataRegistry.Instance.StaffColumnToGroupNamesMapping.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, mappings);

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "is controller";
			group.GG_ExternalId = "";

			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals(true, testForm.MembersModuleButtonGrid.AttachButton.Enabled);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				testForm.MembersModuleButtonGrid.AttachButton.PerformClick();
				AssertEquals("Cannot add staff members to SCIM-mapped groups. Please use relevant mapped checkboxes.", UnitTestUserNotification.Instance.LastMessage.Text);
			}

			group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "is controller";
			group.GG_ExternalId = "123";

			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals(true, testForm.MembersModuleButtonGrid.AttachButton.Enabled);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.OK);

				testForm.MembersModuleButtonGrid.AttachButton.PerformClick();
				AssertEquals("Cannot add staff members to SCIM-mapped groups. Please use relevant mapped checkboxes.", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		public void TestDomainNameDropEditVisibility()
		{
			var adRegistry = ObjectFactory.Get<IADRegistry>();
			var group = Factory.NewWithValidTestData<GlbGroup>();

			//AD Enabled, no domains
			adRegistry.IsIntegrationEnabled = true;
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should not show when Domain Credentials registry is not set even if AD Integration is enabled", false, testForm.DomainNameDropEdit.Visible);
			}

			//AD Enabled, one domain
			var domain1 = ObjectFactory.Get<IDomainCredentials>();
			domain1.DomainName = "domain1";
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should show when there is one or more Domain Credentials and AD Integration is enabled", true, testForm.DomainNameDropEdit.Visible);
			}

			EnvProxy.SetHostedLocationForTest("SYD");

			//AD Enabled, one domain, is hosted, login as non-support
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				using (var testForm = NewGlbGroupForm(group))
				{
					testForm.Show();
					AssertEquals("Is Hosted", true, EnvProxy.IsHostedWithCargowise);
					AssertEquals("Not support user", false, Env.CurrentUser.IsSupportUser);
					AssertEquals("Should not show when it is hosted", false, testForm.DomainNameDropEdit.Visible);
				}
			}

			//AD Enabled, one domain, is hosted, login as CW1 Support
			using (EnvProxy.Instance.SetTemporaryUserContext(User.SupportUserName, Env.CurrentBranch.PK, Env.CurrentDepartment.PK))
			{
				using (var testForm = NewGlbGroupForm(group))
				{
					testForm.Show();
					AssertEquals("Is Hosted", true, EnvProxy.IsHostedWithCargowise);
					AssertEquals("Is support user", true, Env.CurrentUser.IsSupportUser);
					AssertEquals("Should show when CWSupport login even it is hosted", true, testForm.DomainNameDropEdit.Visible);
				}
			}
			EnvProxy.SetHostedLocationForTest("");
			AssertEquals("Not IsHosted", false, EnvProxy.IsHostedWithCargowise);

			//AD Enabled, two domains
			var domain2 = ObjectFactory.Get<IDomainCredentials>();
			domain2.DomainName = "domain2";
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1, domain2 };
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should show when there are one or more Domain Credentials and AD Integration enabled", true, testForm.DomainNameDropEdit.Visible);
			}

			//AD Disabled, two domains
			adRegistry.IsIntegrationEnabled = false;
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should show when there are one or more Domain Credentials even if AD Integration is disabled", true, testForm.DomainNameDropEdit.Visible);
			}

			//AD Disabled, one domain
			adRegistry.IsIntegrationEnabled = false;
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domain1 };
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should show when there are one or more Domain Credentials even if AD Integration is disabled", true, testForm.DomainNameDropEdit.Visible);
			}

			//AD Disabled, no domains
			adRegistry.IsIntegrationEnabled = false;
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>();
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should not show when Domain Credentials registry is not set, regardless of AD Integration status", false, testForm.DomainNameDropEdit.Visible);
			}
		}

		[RequiresSTA]
		public void TestADLinkedCheckBoxVisibility()
		{
			var adRegistry = ObjectFactory.Get<IADRegistry>();
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var sysGroup = Factory.New<GlbGroup>();
			sysGroup.GG_IsSystemDefined = true;
			AssertNotNull(sysGroup);

			//AD Enabled
			adRegistry.IsIntegrationEnabled = true;
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should show Is AD Linked for normal group", true, testForm.ADLinkedCheckBox.Visible);
			}

			using (var testForm = NewGlbGroupForm(sysGroup))
			{
				testForm.Show();
				AssertEquals("Should not show Is AD Linked for system group", false, testForm.ADLinkedCheckBox.Visible);
			}

			//AD Disabled
			adRegistry.IsIntegrationEnabled = false;
			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should not show Is AD Linked when AD disabled", false, testForm.ADLinkedCheckBox.Visible);
			}

			using (var testForm = NewGlbGroupForm(sysGroup))
			{
				testForm.Show();
				AssertEquals("Should not show Is AD Linked for system group", false, testForm.ADLinkedCheckBox.Visible);
			}
		}

		public void TestFormCaption()
		{
			using (var groupForm = (GlbGroupForm)GetFormToBashCore())
			{
				AssertEquals("Form caption is Group", "Group", groupForm.FormCaption);
			}

			var group = Factory.New<GlbGroup>();
			group.GG_Desc = "testGroup";

			using (var groupForm = NewGlbGroupForm(group))
			{
				AssertEquals("Form caption is Group with Desc", "Group " + group.GG_Desc, groupForm.FormCaption);
			}
		}

		public void TestVanishingControlsFromDesignerBug()
		{
			using (GlbGroupForm groupForm = (GlbGroupForm)GetFormToBashCore())
			{
				groupForm.Show();
				AssertNotNull("Security Tree View should not be null.", groupForm.SecurityTreeView);
				AssertNotNull("Security Tree View should be bound.", groupForm.SecurityTreeView.BindingContext);
				AssertEquals("Security Tree View should be anchored", (((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
					| System.Windows.Forms.AnchorStyles.Left) | System.Windows.Forms.AnchorStyles.Right),
					groupForm.SecurityTreeView.Anchor);
			}
		}

		[ExpectException(typeof(CannotDeleteException))]
		public void TestAttemptDeleteFromAllUsers()
		{
			GlbGroup group = Factory.LoadTop1<GlbGroup>(new ZQuery(GlbGroupSchema.GG_Code, "ALL"));
			using (GlbGroupForm groupForm = NewGlbGroupForm(group))
			{
				groupForm.Show();
				group.Staff.RemoveAll();
				Assert("Staff members in All users group should not have been removed", group.Staff.Count > 0);
				AssertEquals("Cannot remove staff member from the ALL Users group", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		[RequiresSTA]
		public void TestCreateGroupForm()
		{
			using (GlbGroupForm groupForm = (GlbGroupForm)GetFormToBashCore())
			{
				Assert("SecurityTreeView should not be populated", groupForm.SecurityTreeView.Nodes.Count == 0);
				GlbStaffCollection collection = new GlbStaffCollection(Factory);
				collection.AdditionalFilter = new ZQuery(GlbStaffSchema.PK, groupForm.fGroupSecurity.UserPK);
				AssertEquals("Fake staff member used for group form should not be in database", 0, collection.Count);
				AssertEquals("Security Branch PK should be current branch", Env.CurrentBranch.PK, groupForm.fGroupSecurity.BranchPK);
				AssertEquals("Security Department PK should be current branch", Env.CurrentDepartment.PK, groupForm.fGroupSecurity.DepartmentPK);
				AssertEquals("Security Company PK should be current branch", Env.CurrentCompany.PK, groupForm.fGroupSecurity.CompanyPK);
				Assert("Security caching should not be enabled", !groupForm.fGroupSecurity.CachingEnabled);
			}
		}

		[RequiresSTA]
		public void TestSelectIndexChanged()
		{
			using (GlbGroupForm groupForm = (GlbGroupForm)GetFormToBashCore())
			{
				groupForm.GroupTabControl.SelectedIndex = 0;
				AssertEquals("Selected tab should be MembersTabPage", groupForm.MembersTabPage, groupForm.GroupTabControl.SelectedTab);
				groupForm.SecurityBranchGuidFindBox.CurrentCode = "";
				groupForm.SecurityDepartmentGuidFindBox.CurrentCode = "";

				groupForm.GroupTabControl.SelectedIndex = 1;
				AssertEquals("Selected tab should be SecurityTabPage", groupForm.SecurityTabPage, groupForm.GroupTabControl.SelectedTab);
				AssertEquals("Branch code to filter by is current branch", Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK, groupForm.Group.SecurityBranch);
				AssertEquals("Department code to filter by is current department", Enterprise.MasterFiles.Business.GlbDepartment.CurrentDepartment.PK, groupForm.Group.SecurityDepartment);
			}
		}

		[ExpectNoExceptions]
		[RequiresSTA]
		public void TestNoException_OpeningSecurityRightsTabPage_ForNewGroup()
		{
			GlbGroup group1 = Factory.New<GlbGroup>();

			using (GlbGroupForm testForm = NewGlbGroupForm(group1))
			{
				testForm.Show();
				Application.DoEvents();
				testForm.GroupTabControl.SelectTab("SecurityTabPage");
				Application.DoEvents();
			}
		}

		[RequiresSTA]
		public void TestSecurityTreeGlowApplicationCheckPoint_WhenRegistryDisabled()
		{
			var group1 = Factory.New<GlbGroup>();

			var mock = new Mock<IAccountingRegistryProvider>();
			mock.Setup(m => m.EnablePayablesInvoiceProcessingPortal).Returns(false);
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var groupForm = NewGlbGroupForm(group1))
				{
					groupForm.Show();
					groupForm.SecurityTabPage.Select();
					groupForm.GroupTabControl.SelectedIndex = 1;
					var securityRecordParent = Factory.New<GlbSecurity>();
					securityRecordParent.GU_SecurityItemIsAllowed = true;
					securityRecordParent.GU_SecurityRight = groupForm.SecurityTreeView.Nodes[0].Text;
					securityRecordParent.GU_GG = group1.PK;
					group1.SecurityPermissions.Add(securityRecordParent);

					var glowSecurityCheckpointRootNode = groupForm.SecurityTreeView
						.Nodes.Cast<TreeNode>().FirstOrDefault(node => node.Text == "Web Portal Management");
					AssertNull(glowSecurityCheckpointRootNode);
				}
			}
		}

		[RequiresSTA]
		public void TestSecurityTreeGlowApplicationCheckPoint_WhenRegistryEnabled()
		{
			var group1 = Factory.New<GlbGroup>();

			var mock = new Mock<IAccountingRegistryProvider>();
			mock.Setup(m => m.EnablePayablesInvoiceProcessingPortal).Returns(true);
			using (ObjectFactory.Substitute(mock.Object))
			{
				using (var groupForm = NewGlbGroupForm(group1))
				{
					groupForm.Show();
					groupForm.SecurityTabPage.Select();
					groupForm.GroupTabControl.SelectedIndex = 1;
					var securityRecordParent = Factory.New<GlbSecurity>();
					securityRecordParent.GU_SecurityItemIsAllowed = true;
					securityRecordParent.GU_SecurityRight = groupForm.SecurityTreeView.Nodes[0].Text;
					securityRecordParent.GU_GG = group1.PK;
					group1.SecurityPermissions.Add(securityRecordParent);

					var glowSecurityCheckpointRootNode = groupForm.SecurityTreeView
						.Nodes.Cast<TreeNode>().First(node => node.Text == "Web Portal Management");
					AssertNotNull(glowSecurityCheckpointRootNode);
					var glowSecurityCheckpointChildNodes = glowSecurityCheckpointRootNode.Nodes.Cast<TreeNode>();
					var childApplicationCheckpoints = ApplicationCheckpoints.GetAll();
					AssertEquals("Should contain the same number of application points from glow integration dll", childApplicationCheckpoints.Count, glowSecurityCheckpointChildNodes.Count());

					groupForm.SecurityTreeView.SelectedNode = glowSecurityCheckpointRootNode;
					FireAfterSelect(glowSecurityCheckpointRootNode);
					var rightPanel = groupForm.Find(x => x.Name == "MainSecurityRightsPanel").Cast<ZPanel>().Single();
					Assert("root node panel should be invisible", !rightPanel.Visible);

					foreach (TreeNode childNode in glowSecurityCheckpointRootNode.Nodes)
					{
						groupForm.SecurityTreeView.SelectedNode = childNode;
						FireAfterSelect(childNode);
						rightPanel = groupForm.Find(x => x.Name == "MainSecurityRightsPanel").Cast<ZPanel>().Single();
						Assert("child node panel should be visible", rightPanel.Visible);
					}
				}
			}
		}

		public void TestSecurityTreeViewAfterSelect()
		{
			GlbGroup group1 = Factory.New<GlbGroup>();

			using (GlbGroupForm groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.SecurityTabPage.Select();
				groupForm.GroupTabControl.SelectedIndex = 1;
				GlbSecurity securityRecordParent = Factory.New<GlbSecurity>();
				securityRecordParent.GU_SecurityItemIsAllowed = true;
				securityRecordParent.GU_SecurityRight = groupForm.SecurityTreeView.Nodes[0].Text;
				securityRecordParent.GU_GG = group1.PK;
				group1.SecurityPermissions.Add(securityRecordParent);

				var schedulesSecurityNode = groupForm.SecurityTreeView
					.Nodes.Cast<TreeNode>().First(node => node.Text == "Operate").Nodes.Cast<TreeNode>().First(node => node.Text == "Schedules");
				AssertNotNull("We should be able to find the 'Schedules' Security Node within the 'Operate' Node, but instead we could not! Bad!", schedulesSecurityNode);

				GlbSecurity securityRecordChild = Factory.New<GlbSecurity>();
				securityRecordChild.GU_SecurityItemIsAllowed = true;
				securityRecordChild.GU_SecurityRight = schedulesSecurityNode.Text;
				securityRecordChild.GU_GG = group1.PK;
				group1.SecurityPermissions.Add(securityRecordChild);

				groupForm.GroupTabControl.SelectedIndex = 1;
				AssertEquals("Selected tab should be SecurityTabPage", groupForm.SecurityTabPage, groupForm.GroupTabControl.SelectedTab);

				groupForm.SecurityTreeView.SelectedNode = groupForm.SecurityTreeView.Nodes[0];
				FireAfterSelect(groupForm.SecurityTreeView.Nodes[0]);
				AssertEquals("First node should be selected", groupForm.SecurityTreeView.Nodes[0], groupForm.SecurityTreeView.SelectedNode);
				AssertEquals("Security function name should be the same as selected security node", groupForm.SecurityFunctionPathLabel.Text, groupForm.SecurityTreeView.SelectedNode.Text);

				groupForm.SecurityTreeView.SelectedNode = groupForm.SecurityTreeView.SelectedNode.Nodes[0].Nodes[0];
				FireAfterSelect(schedulesSecurityNode);
				string securityPathName = groupForm.SecurityTreeView.SelectedNode.Parent.Text + "|" + groupForm.SecurityTreeView.SelectedNode.Text;
				AssertEquals("Security function name should be", securityPathName, groupForm.SecurityFunctionPathLabel.Text);

				AssertEquals("One relevant security permissions should be shown", 1, groupForm.Group.SecurityPermissionsView.Count);

				GlbSecurity securityRecordSecondChild = Factory.New<GlbSecurity>();
				securityRecordSecondChild.GU_SecurityItemIsAllowed = true;
				securityRecordSecondChild.GU_SecurityRight = schedulesSecurityNode.Text;
				securityRecordSecondChild.GU_GG = group1.PK;
				securityRecordSecondChild.GU_GB = Env.CurrentBranch.PK;
				group1.SecurityPermissions.Add(securityRecordSecondChild);

				FireAfterSelect(schedulesSecurityNode);
				AssertEquals("Two relevant security permissions should be shown", 2, groupForm.Group.SecurityPermissionsView.Count);
			}
		}

		void FireAfterSelect(TreeNode treeNode)
		{
			TreeView treeView = treeNode.TreeView;
			treeView.GetType().GetMethod("OnAfterSelect", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
				.Invoke(treeView, new object[] { new TreeViewEventArgs(treeNode, TreeViewAction.ByMouse) });
		}

		#region Refresh Allowed Organisations And Warehouses Control

		public void TestRefreshAllowedOrgsAndWarehousesSecurityGrid()
		{
			GlbGroup group = Factory.New<GlbGroup>();

			GlbSecurity security1 = CreateSecurityItem(group, Factory.New<OrgHeader>(), GlbSecurity.AllowedPrincipalsSecurityRightName);
			GlbSecurity security2 = CreateSecurityItem(group, Factory.New<OrgHeader>(), GlbSecurity.AllowedClientsSecurityRightName);
			GlbSecurity security3 = CreateSecurityItem(group, Factory.New<OrgHeader>(), GlbSecurity.AllowedWarehousesSecurityRightName); //bind to OrgHeader because no access to WhsWarehouse in there

			using (GlbGroupForm groupForm = NewGlbGroupForm(group))
			{
				groupForm.Show();
				groupForm.GroupTabControl.SelectedTab = groupForm.SecurityTabPage;

				// Could create correct Item (WhsWarehouse) only there
				groupForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.WhsAllowedWarehouses));
				security3.GU_ItemGUID = (Factory.New(group.SecurityAllowedOrgsAndWarehousesView.TypeOfElements)).PK;

				groupForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.AgencyPrincipalAccess));
				AssertEquals("Collection of allowed principals should contain only one security item", 1, group.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertCollectionContains("Collection of allowed principals should contain the correct security item", security1, group.SecurityAllowedOrgsAndWarehousesView);

				groupForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.WhsAllowedClients));
				AssertEquals("Collection of allowed clients should contain only one security item", 1, group.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertCollectionContains("Collection of allowed clients should contain the correct security item", security2, group.SecurityAllowedOrgsAndWarehousesView);

				groupForm.RefreshAllowedOrgsAndWarehousesSecurityGrid(GetSecurityPointNode(Env.Security.WhsAllowedWarehouses));
				AssertEquals("Collection of allowed warehouses should contain only one security item", 1, group.SecurityAllowedOrgsAndWarehousesView.Count);
				AssertCollectionContains("Collection of allowed warehouses should contain the correct security item", security3, group.SecurityAllowedOrgsAndWarehousesView);
			}
		}

		ZSecurityPointNode GetSecurityPointNode(SecurityCheckpoint securityCheckPoint)
		{
			return new ZSecurityPointNode(securityCheckPoint.DisplayText, securityCheckPoint);
		}

		GlbSecurity CreateSecurityItem(GlbGroup group, BusinessObject bizO, ZString securityRight)
		{
			GlbSecurity security = group.SecurityAllowedOrgsAndWarehousesView.AddNew(); //Factory.New<GlbSecurity>();
			security.GU_ItemGUID = bizO.PK;
			security.GU_GG = group.PK;
			security.GU_SecurityRight = securityRight;
			security.GU_SecurityItemIsAllowed = true;
			return security;
		}

		#endregion

		public void TestRefreshGroupSecurityLabel()
		{
			GlbGroup group1 = Factory.New<GlbGroup>();
			Factory.Save();

			using (GlbGroupForm groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.SecurityTabPage.Select();
				groupForm.GroupTabControl.SelectedIndex = 1;
				GlbSecurity securityRecordWithBranch = Factory.New<GlbSecurity>();
				securityRecordWithBranch.GU_SecurityItemIsAllowed = false;
				securityRecordWithBranch.GU_SecurityRight = ((SecurityCheckpoint)groupForm.SecurityTreeView.Nodes[0].Tag).Code;
				securityRecordWithBranch.GU_GG = group1.PK;
				securityRecordWithBranch.GU_GB = Env.CurrentBranch.PK;
				group1.SecurityPermissions.Add(securityRecordWithBranch);

				GlbSecurity securityRecordWithoutBranch = Factory.New<GlbSecurity>();
				securityRecordWithoutBranch.GU_SecurityItemIsAllowed = true;
				securityRecordWithoutBranch.GU_SecurityRight = ((SecurityCheckpoint)groupForm.SecurityTreeView.Nodes[0].Tag).Code;
				securityRecordWithoutBranch.GU_GG = group1.PK;
				group1.SecurityPermissions.Add(securityRecordWithoutBranch);

				groupForm.GroupTabControl.SelectedIndex = 1;
				groupForm.SecurityTreeView_AfterSelect(null, new TreeViewEventArgs(groupForm.SecurityTreeView.Nodes[0], TreeViewAction.ByMouse));
				AssertEquals("First node should be selected", groupForm.SecurityTreeView.Nodes[0], groupForm.SecurityTreeView.SelectedNode);
				AssertEquals("Filtering by branch and department, permission should be denied", "No", groupForm.GroupSecurityRightLabel.Text);

				securityRecordWithBranch.GU_SecurityItemIsAllowed = true;
				groupForm.SecurityTreeView_AfterSelect(null, new TreeViewEventArgs(groupForm.SecurityTreeView.Nodes[0], TreeViewAction.ByMouse));
				AssertEquals("Filtering by branch and department, permission should be allowed", "Yes", groupForm.GroupSecurityRightLabel.Text);

				securityRecordWithBranch.GU_SecurityItemIsAllowed = false;
				groupForm.SecurityTreeView.SelectedNode = groupForm.SecurityTreeView.SelectedNode.Nodes[0];
				groupForm.SecurityGrid_CurrentCellChanged(null, EventArgs.Empty);
				AssertEquals("Filtering by branch and department, permission should be denied", "No", groupForm.GroupSecurityRightLabel.Text);

				groupForm.Group.SecurityBranch = ZGuid.Empty;
				groupForm.SecurityTreeView_AfterSelect(null, new TreeViewEventArgs(groupForm.SecurityTreeView.Nodes[0], TreeViewAction.ByMouse));
				AssertEquals("No branch specified, permissions not found", "N/A", groupForm.GroupSecurityRightLabel.Text);
			}
		}

		[RequiresSTA]
		public void TestSecurityIsSetForGroupSecurityPermissions()
		{
			GlbGroup group = Factory.New<GlbGroup>();
			using (GlbGroupForm form = NewGlbGroupForm(group))
			{
				AssertEquals("Security should be set on group.SecurityPermissions.", form.fGroupSecurity, group.SecurityPermissions.Security);
				bool foundKeyWithItemGuid = false;
				foreach (CheckpointLookupKey key in group.SecurityPermissions.Security.CheckPointLookUpTable_ForTest.Keys)
				{
					if (key.ItemGuid != Guid.Empty)
					{
						foundKeyWithItemGuid = true;
						break;
					}
				}
				AssertEquals("Security should be populated.", false, foundKeyWithItemGuid);
			}
		}

		public void TestTabPageSecurity()
		{
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			GlbStaff staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				Env.Security.GroupsModify.IsAllowed = false;
				AssertSecuredTabPagesEnabledState(group, false);

				Env.Security.GroupsModify.IsAllowed = true;
				AssertSecuredTabPagesEnabledState(group, true);

				Env.Security.GroupsModify.IsAllowed = false;
				Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(group.PK.ToGuid()).IsAllowed = true;
				AssertSecuredTabPagesEnabledState(group, true);
			}
		}

		[RequiresSTA]
		public void TestEffectiveSecurityRightsGroupBoxTextIsTrimmedOnExcessLength()
		{
			using (var form = NewGlbGroupForm(Factory.NewWithValidTestData<GlbGroup>()))
			{
				form.Show();
				var groupBox = form.EffectiveSecurityRightsGroupBox;
				var node = new ZSecurityPointNode("some not very long text", Env.Security.WhsDiagnostic);
				var nodeWithLongText = new ZSecurityPointNode("some very long text aaaaaaaaaa aaaaaaaaa aaaaaaaaa aaaaaaaa aaaaaaaa aaaaaaa aaaaaa aaaaa aaaaaaa aaaaaa aaaaaaaa aaaaaaa aaaaaaa", Env.Security.WhsDiagnostic);

				form.GroupTabControl.SelectedTab = form.SecurityTabPage;

				form.SecurityTreeView.Nodes.Add(node);
				form.SecurityTreeView.SelectedNode = node;

				Assert(node.IsSelected);
				AssertEquals("Short string should not be truncated", "some not very long text", groupBox.Text);

				form.SecurityTreeView.Nodes.Add(nodeWithLongText);
				form.SecurityTreeView.SelectedNode = nodeWithLongText;

				Assert(nodeWithLongText.IsSelected);

				var textRegionWidth = groupBox.Width * 2 - ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
				var textWidth = TextRenderer.MeasureText(groupBox.Text, groupBox.Font).Width;

				Assert(string.Format("Long string should be truncated to {0} pixels", textRegionWidth), textWidth <= textRegionWidth);
				AssertEquals("truncated long string should end with '...'", "...", groupBox.Text.Substring(groupBox.Text.Length - 3));
			}
		}

		public void TestDocDataPlugIn()
		{
			using (GlbGroupForm form = NewGlbGroupForm(Factory.NewWithValidTestData<GlbGroup>()))
			{
				AssertNotNull(form.PlugIns.GetPlugIn(ControllerIDs.DocDataPlugIn));
			}
		}

		public void TestCanReactivateGroup()
		{
			UnitTestUserNotification.Instance.ClearMessagesAndAnswers();
			GlbGroup group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Desc = "Test";
			group.Staff.Add(GlbStaff.CurrentUser);
			group.GG_IsActive = false;
			Factory.Save();

			using (GlbGroupForm form = NewGlbGroupForm(group))
			{
				form.Show();
				group.GG_IsActive = true;
				form.FireSaveButton();
				Assert("No error message Shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}

			group.GG_IsActive = false;
			Factory.Save();
			using (GlbGroupForm form = NewGlbGroupForm(group))
			{
				form.Show();
				group.Staff.Add(Factory.NewWithValidTestData<GlbStaff>());
				group.GG_IsActive = true;
				form.FireSaveButton();
				Assert("No error message Shown", UnitTestUserNotification.Instance.LastMessage.WasNone);
			}
		}

		public void TestChangeGroupName_WhenConflictWithAD_ShouldNotSave()
		{
			AssertWhenIdentityConflict_ShouldContinueWithSave(true, ContinueWithSave.No);
		}

		public void TestChangeGroupName_WhenNoConflictWithAD_ShouldSave()
		{
			AssertWhenIdentityConflict_ShouldContinueWithSave(false, ContinueWithSave.Yes);
		}

		public void TestOrganisationsButtonsWithSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsManageOSMG.IsAllowed = false;

			using (var form = NewGlbGroupForm(group))
			{
				form.Show();
				var tabPages = form.Controls.Find("OrganisationsTabPage", true);
				AssertEquals(1, tabPages.Length);
				AssertEquals("OrganisationsTabPage", false, form.OrganisationsModuleButtonGrid.AttachButtonForTest.Enabled);
			}
		}

		public void TestTabPageMembersWithViewSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewMembers.IsAllowed = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var form = NewGlbGroupForm(group))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				form.GroupTabControl.SelectedTab = form.MembersTabPage;

				AssertEquals("coveringLabel", form.MembersTabPage.Controls[0].Name);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Group -> View -> Members
", form.MembersTabPage.Controls[0].Text);
			}
		}

		[RequiresSTA]
		public void TestTabPageOrganizationsWithViewSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewOrganisations.IsAllowed = false;

			using (var form = NewGlbGroupForm(group))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				form.GroupTabControl.SelectedTab = form.OrganisationsTabPage;

				AssertEquals("coveringLabel", form.OrganisationsTabPage.Controls[0].Name);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Group -> View -> Organizations
", form.OrganisationsTabPage.Controls[0].Text);
			}
		}

		public void TestTabPageSecurityRightsWithViewSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewSecurityRights.IsAllowed = false;

			using (var form = NewGlbGroupForm(group))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				form.GroupTabControl.SelectedTab = form.SecurityTabPage;

				AssertEquals("coveringLabel", form.SecurityTabPage.Controls[0].Name);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Group -> View -> Security Rights
", form.SecurityTabPage.Controls[0].Text);
			}
		}

		[RequiresSTA]
		public void TestTabPageMembersWithModifySecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewMembers.IsAllowed = true;
			Env.Security.GroupsModifyMembers.IsAllowed = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var form = NewGlbGroupForm(group))
			{
				form.Show();
				form.GroupTabControl.SelectedTab = form.MembersTabPage;

				Assert(form.tabSecurityLabel.Visible);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(form.MembersModuleButtonGrid.ReadOnly);
				AssertNotEquals("coveringLabel", form.MembersTabPage.Controls[0].Name);
			}
		}

		[RequiresSTA]
		public void TestTabPageOrganizationsWithModifySecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewOrganisations.IsAllowed = true;
			Env.Security.GroupsModifyOrganisations.IsAllowed = false;

			var staff = Factory.NewWithValidTestData<GlbStaff>();
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			using (var form = NewGlbGroupForm(group))
			{
				form.Show();
				form.GroupTabControl.SelectedTab = form.OrganisationsTabPage;

				Assert(form.tabSecurityLabel.Visible);
				Assert(form.OrganisationsModuleButtonGrid.Enabled);
				Assert(form.OrganisationsModuleButtonGrid.ReadOnly);
				AssertNotEquals("coveringLabel", form.OrganisationsTabPage.Controls[0].Name);
			}
		}

		public void TestTabPageSecurityRightsWithModifySecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Factory.Save();

			using (CurrentUserChanger.SwitchToNewUserTemporarily(staff.GS_LoginName))
			{
				Env.Security.GroupsViewSecurityRights.IsAllowed = true;
				Env.Security.GroupsModifySecurityRights.IsAllowed = false;

				using (var form = NewGlbGroupForm(group))
				{
					form.Show();
					form.GroupTabControl.SelectedTab = form.SecurityTabPage;

					Assert(form.tabSecurityLabel.Visible);
					Assert(form.SecurityTreeView.Enabled);
					Assert(form.SecurityTreeView.ReadOnly);
					AssertNotEquals("coveringLabel", form.SecurityTabPage.Controls[0].Name);
				}
			}
		}

		public void TestTabPageCustomFieldsWithModifySecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewCustomFields.IsAllowed = true;
			Env.Security.GroupsModifyCustomFields.IsAllowed = false;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			var template = Factory.NewWithValidTestData<ProcessTaskTemplate>();
			template.P0_ProcessType = WorkflowDescriptors.GlbGroupWorkflowDescriptorCode;
			var customField = template.GenCustomColumnDefinitions.AddNew();
			customField.XC_Name = "StringField";
			customField.XC_Type = AddOnColumnDataType.Codes.String;

			Factory.Save();

			using (var form = NewGlbGroupForm(group))
			{
				form.Show();
				form.GroupTabControl.SelectedTab = form.CustomFieldsTabPage;

				Assert("Should show security label", form.tabSecurityLabel.Visible);
				AssertNotEquals("coveringLabel", form.CustomFieldsTabPage.Controls[0].Name);
			}
		}

		public void TestTabPageCustomFieldsWithViewSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			Env.Security.GroupsViewCustomFields.IsAllowed = false;
			ObjectFactory.Get<IBMSRegistry>().BufferManagementEnabled = true;

			using (var form = NewGlbGroupForm(group))
			{
				form.DisplayMode = ODisplayMode.ReadOnly;
				form.Show();
				form.GroupTabControl.SelectedTab = form.CustomFieldsTabPage;

				AssertEquals("coveringLabel", form.CustomFieldsTabPage.Controls[0].Name);
				AssertEquals(@"You do not have the appropriate security rights to run this function.

If you require access to this function, ask your system administrator to change either your Staff or Group Security Rights to allow access to:

Maintain -> User Admin -> Group -> View -> Custom Fields
", form.CustomFieldsTabPage.Controls[0].Text);
			}
		}

		[RequiresSTA]
		public void TestOnLoad()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.IsCancelled = false;

			using (var form = NewGlbGroupForm(group))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				form.GroupTabControl.SelectedTab = form.MembersTabPage;
			}

			group.IsCancelled = true;

			using (var form = NewGlbGroupForm(group))
			{
				form.DisplayMode = ODisplayMode.Delete;
				form.Show();
				form.GroupTabControl.SelectedTab = form.MembersTabPage;

				Assert(form.MembersModuleButtonGrid.ShowAttachButton);
				Assert(!form.MembersModuleButtonGrid.AttachButtonForTest.Enabled);
				Assert(form.MembersModuleButtonGrid.ShowDetachButton);
				Assert(!form.MembersModuleButtonGrid.DetachButtonForTest.Enabled);
			}
		}

		[RequiresSTA]
		public void TestNonGroupOwnersCannotEditGroupOwnersAndMembers()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ABC";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.GroupsView.IsAllowed = true;
			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.GroupsModifyMembers.IsAllowed = false;
			Env.Security.GroupsModifySecurityRights.IsAllowed = false;
			Factory.Save();

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				form.Show();

				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You do not have security rights to edit this tab page.", form.tabSecurityLabel.Text);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(form.MembersModuleButtonGrid.ReadOnly);

				form.GroupTabControl.SelectedTab = form.GroupOwnersTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You do not have security rights to edit this tab page.", form.tabSecurityLabel.Text);
				AssertNotEquals("coveringLabel", form.SecurityTabPage.Controls[0].Name);
				Assert(form.GroupOwnersSecurityControl.Enabled);
				Assert(form.GroupOwnersSecurityControl.changeOthersSecurityGrid.ReadOnly);
				Assert(!form.GroupOwnersSecurityControl.addStaffButton.Enabled);
				Assert(!form.GroupOwnersSecurityControl.addGroupButton.Enabled);
				Assert(!form.GroupOwnersSecurityControl.deleteStaffOrGroupButton.Enabled);
			}
		}

		public void TestNonGroupOwnerWithSecurityRightsCanEditGroupOwners()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ABC";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.GroupsView.IsAllowed = true;
			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.GroupsModifyMembers.IsAllowed = true;
			Env.Security.GroupsModifyGroupOwners.IsAllowed = true;
			Factory.Save();

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				form.Show();

				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				Assert(!form.tabSecurityLabel.Visible);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(!form.MembersModuleButtonGrid.ReadOnly);

				form.GroupTabControl.SelectedTab = form.GroupOwnersTabPage;
				Assert(!form.tabSecurityLabel.Visible);
				Assert(form.GroupOwnersSecurityControl.Enabled);
				Assert(!form.GroupOwnersSecurityControl.changeOthersSecurityGrid.ReadOnly);
				Assert(form.GroupOwnersSecurityControl.addStaffButton.Enabled);
				Assert(form.GroupOwnersSecurityControl.addGroupButton.Enabled);
				Assert(form.GroupOwnersSecurityControl.deleteStaffOrGroupButton.Enabled);
			}
		}

		public void TestGroupOwnerWithSecurityRightCanEditGroupOwners()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ABC";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.GroupsView.IsAllowed = true;
			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.GroupsModifyMembers.IsAllowed = false;
			Env.Security.GroupsModifyGroupOwners.IsAllowed = true;
			staff.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			staff.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);
			Factory.Save();

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				form.Show();

				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You are a Group Owner and can modify membership.", form.tabSecurityLabel.Text);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(!form.MembersModuleButtonGrid.ReadOnly);

				form.GroupTabControl.SelectedTab = form.GroupOwnersTabPage;
				Assert(!form.tabSecurityLabel.Visible);
				Assert(form.GroupOwnersSecurityControl.Enabled);
				Assert(!form.GroupOwnersSecurityControl.changeOthersSecurityGrid.ReadOnly);
				Assert(form.GroupOwnersSecurityControl.addStaffButton.Enabled);
				Assert(form.GroupOwnersSecurityControl.addGroupButton.Enabled);
				Assert(form.GroupOwnersSecurityControl.deleteStaffOrGroupButton.Enabled);
			}
		}

		[RequiresSTA]
		public void TestGroupOwnerWithoutSecurityRightCannotEditGroupOwners()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ABC";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.GroupsView.IsAllowed = true;
			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.GroupsModifyMembers.IsAllowed = false;
			Env.Security.GroupsModifyGroupOwners.IsAllowed = false;
			staff.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			staff.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);
			Factory.Save();

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				form.Show();

				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You are a Group Owner and can modify membership.", form.tabSecurityLabel.Text);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(!form.MembersModuleButtonGrid.ReadOnly);

				form.GroupTabControl.SelectedTab = form.GroupOwnersTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You do not have security rights to edit this tab page.", form.tabSecurityLabel.Text);
				AssertNotEquals("coveringLabel", form.SecurityTabPage.Controls[0].Name);
				Assert(form.GroupOwnersSecurityControl.Enabled);
				Assert(form.GroupOwnersSecurityControl.changeOthersSecurityGrid.ReadOnly);
				Assert(!form.GroupOwnersSecurityControl.addStaffButton.Enabled);
				Assert(!form.GroupOwnersSecurityControl.addGroupButton.Enabled);
				Assert(!form.GroupOwnersSecurityControl.deleteStaffOrGroupButton.Enabled);
			}
		}

		public void TestGroupOwnerSecurity()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_Code = "ABC";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.GroupsView.IsAllowed = true;
			Env.Security.GroupsModify.IsAllowed = false;
			Env.Security.GroupsModifyMembers.IsAllowed = false;
			Env.Security.GroupsModifyGroupOwners.IsAllowed = false;
			Env.Security.GroupsModifySecurityRights.IsAllowed = false;
			staff.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
			staff.SecurityChangeOthersView.AddSecurityToChangeOtherGroup(group.GG_Code);
			Factory.Save();

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				form.Show();

				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You are a Group Owner and can modify membership.", form.tabSecurityLabel.Text);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(!form.MembersModuleButtonGrid.ReadOnly);

				form.GroupTabControl.SelectedTab = form.SecurityTabPage;
				Assert(form.tabSecurityLabel.Visible);
				AssertEquals("You do not have security rights to edit this tab page.", form.tabSecurityLabel.Text);
				Assert(form.SecurityTreeView.ReadOnly);

				form.GroupTabControl.SelectedTab = form.zStmNoteTabPage1;
				Application.DoEvents();
				Assert(!form.tabSecurityLabel.Visible);
				var zStmNoteUserControl = form.zStmNoteTabPage1.Controls.OfType<ZStmNoteUserControl>().First();
				var field = typeof(ZStmNoteUserControl).GetField("NoteGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var noteGrid = (ZStmNoteGrid)field.GetValue(zStmNoteUserControl);
				Assert(!noteGrid.ReadOnly);
			}

			Env.Security.GroupsModify.IsAllowed = true;

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				form.Show();
				form.GroupTabControl.SelectedTab = form.zStmNoteTabPage1;
				Application.DoEvents();
				Assert(!form.tabSecurityLabel.Visible);
				var zStmNoteUserControl = form.zStmNoteTabPage1.Controls.OfType<ZStmNoteUserControl>().First();
				var field = typeof(ZStmNoteUserControl).GetField("NoteGrid", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
				var noteGrid = (ZStmNoteGrid)field.GetValue(zStmNoteUserControl);
				Assert(!noteGrid.ReadOnly);
			}
		}

		[RequiresSTA]
		public void TestSecurityGroup()
		{
			TestNonSecurityGroup(false);
		}

		[RequiresSTA]
		public void TestNonSecurityGroup()
		{
			TestNonSecurityGroup(true);
		}

		[RequiresSTA]
		public void TestNonSecurityGroup(bool nonSecurityGroupInitialValue)
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.IsNonSecurityGroup = nonSecurityGroupInitialValue;
			Factory.Save();
			Assert(group.IsNonSecurityGroup == nonSecurityGroupInitialValue);
			group.GG_Code = "ABC";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			Env.Security.GroupsView.IsAllowed = true;
			Env.Security.GroupsViewMembers.IsAllowed = true;
			Env.Security.GroupsModify.IsAllowed = true;
			Env.Security.GroupsModifyMembers.IsAllowed = true;
			Env.Security.GroupsModifySecurityRights.IsAllowed = true;

			GlbSecurity security = Factory.NewWithValidTestData<GlbSecurity>();
			security.GU_SecurityRight = "Operations";
			security.GU_GG = group.PK;
			GlbBranch branch = Factory.NewWithValidTestData<GlbBranch>();
			security.GU_GB = branch.PK;
			security.GU_SecurityItemIsAllowed = true;
			group.SecurityPermissions.Add(security);

			const string confirmString = "CONFIRM";
			const string checkNonSecurityGroupPrompt = "Changing this Group to a Non-Security Group will remove Security Rights previously assigned.";
			const string uncheckNonSecurityGroupPrompt = "When changing a Non-Security Group to a Security Group, previously assigned Security Rights are not restored. Please verify the Security rights that this Group will manage.";
			var checkMessage1 = nonSecurityGroupInitialValue ? uncheckNonSecurityGroupPrompt : checkNonSecurityGroupPrompt;
			var checkMessage2 = nonSecurityGroupInitialValue ? checkNonSecurityGroupPrompt : uncheckNonSecurityGroupPrompt;

			using (var form = NewGlbGroupForm(group))
			using (Env.SetTemporaryUserContext(new UserContext(staff, Env.CurrentBranch.PK, Env.CurrentDepartment.PK)))
			{
				var unitTestUserNotification = UnitTestUserNotification.Instance;
				unitTestUserNotification.ClearMessagesAndAnswers();

				form.Show();
				Application.DoEvents();

				AssertEquals(ZDialogResult.None, unitTestUserNotification.LastMessage.Answer);
				AssertEquals(null, unitTestUserNotification.LastMessage.Text);

				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				Assert(!form.tabSecurityLabel.Visible);
				Assert(form.MembersModuleButtonGrid.Enabled);
				Assert(!form.MembersModuleButtonGrid.ReadOnly);

				form.GroupTabControl.SelectedTab = form.SecurityTabPage;
				Assert(group.IsNonSecurityGroup == nonSecurityGroupInitialValue);
				Assert(group.SecurityPermissions.IsValidationSuspended == nonSecurityGroupInitialValue);

				unitTestUserNotification.AddAnswer(ZDialogResult.Cancel);
				form.NonSecurityCheckBox.Checked = !nonSecurityGroupInitialValue;
				Application.DoEvents();
				AssertEquals(form.NonSecurityCheckBox.Checked, nonSecurityGroupInitialValue);
				AssertEquals(confirmString, unitTestUserNotification.LastConfirmationStringShown);
				AssertEquals(ZDialogResult.Cancel, unitTestUserNotification.LastMessage.Answer);
				AssertEquals(checkMessage1, unitTestUserNotification.LastMessage.Text);
				Assert(group.SecurityPermissions.IsValidationSuspended == nonSecurityGroupInitialValue);
				Assert(!security.IsDeleted);

				unitTestUserNotification.AddAnswer(ZDialogResult.OK);
				form.NonSecurityCheckBox.Checked = !nonSecurityGroupInitialValue;
				Application.DoEvents();
				AssertEquals(form.NonSecurityCheckBox.Checked, !nonSecurityGroupInitialValue);
				AssertEquals(ZDialogResult.OK, unitTestUserNotification.LastMessage.Answer);
				AssertEquals(checkMessage1, unitTestUserNotification.LastMessage.Text);

				Assert(form.tabSecurityLabel.Visible == !nonSecurityGroupInitialValue);
				AssertEquals("Non-Security Groups can't have security rights added.", form.tabSecurityLabel.Text);
				Assert(form.SecurityTreeView.ReadOnly);
				Assert(security.IsDeleted == !nonSecurityGroupInitialValue);
				Assert(group.IsNonSecurityGroup == !nonSecurityGroupInitialValue);
				Assert(group.SecurityPermissions.IsValidationSuspended == !nonSecurityGroupInitialValue);

				form.NonSecurityCheckBox.Checked = nonSecurityGroupInitialValue;
				Application.DoEvents();
				AssertEquals(form.NonSecurityCheckBox.Checked, nonSecurityGroupInitialValue);
				AssertEquals(confirmString, unitTestUserNotification.LastConfirmationStringShown);
				AssertEquals(ZDialogResult.OK, unitTestUserNotification.LastMessage.Answer);
				AssertEquals(checkMessage2, unitTestUserNotification.LastMessage.Text);
				Assert(group.SecurityPermissions.IsValidationSuspended == nonSecurityGroupInitialValue);
				Assert(security.IsDeleted);
			}
		}

		void AssertWhenIdentityConflict_ShouldContinueWithSave(bool isIdentityInConflict, ContinueWithSave shouldContinueWithSave)
		{
			var adEntityProvider = new Mock<IADEntityProvider>(MockBehavior.Strict);
			var adGroup = new Mock<IADEntity>();

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_ActiveDirectoryObjectGuid = ZGuid.NewZGuid();
			Factory.Save();

			ObjectFactory.Substitute(adEntityProvider.Object);
			ObjectFactory.Get<IADRegistry>().IsIntegrationEnabled = true;

			adEntityProvider.Setup(m => m.GetADGroup(group)).Returns(adGroup.Object);
			adGroup.Setup(m => m.IsIdentityInConflict()).Returns(isIdentityInConflict);
			using (var form = new GlbGroupForm(group))
			{
				form.Show();
				group.GG_Desc = "Hobbits";
				var message = string.Format("When {0}in identity conflict, form.FireSaveButton should return {1}", isIdentityInConflict ? "" : "NOT ", shouldContinueWithSave);
				AssertEquals(message, shouldContinueWithSave, form.FireSaveButton());
			}
		}

		[RequiresSTA]
		public void TestTabPageMembersWithUsersFromDifferentDomain_ShowsWarningUponLoadingForm()
		{
			var domainCredentials1 = ObjectFactory.Get<IDomainCredentials>();
			domainCredentials1.DomainName = "domain1";
			domainCredentials1.IsDefaultDomain = true;
			var domainCredentials2 = ObjectFactory.Get<IDomainCredentials>();
			domainCredentials2.DomainName = "domain2";
			domainCredentials2.IsDefaultDomain = false;

			var adRegistry = ObjectFactory.Get<IADRegistry>();
			adRegistry.DomainCredentialsCollection = new List<IDomainCredentials>() { domainCredentials1, domainCredentials2 };
			adRegistry.IsIntegrationEnabled = true;

			var group = Factory.NewWithValidTestData<GlbGroup>();
			group.GG_DomainName = "domain1";
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_DomainName = "domain2";

			var link = Factory.New<GlbGroupLink>();
			link.GK_GS = staff.PK;
			link.GK_GG = group.PK;

			using (var form = NewGlbGroupForm(group))
			{
				form.GroupTabControl.SelectedTab = form.MembersTabPage;
				form.Show();
				var tabPages = form.Controls.Find("MembersTabPage", true);
				var grid = ((ZModuleButtonGrid)tabPages[0].Controls.Find("MembersModuleButtonGrid", true)[0]).InnerGrid;

				Assert("Staff should show warning in list", ((GlbStaff)grid.List[0]).HasRowWarnings);
				AssertEquals("Message should be correct", "This membership cannot be synchronized with Active Directory because the group and the staff belong to different domains.", ((GlbStaff)grid.List[0]).RowWarnings.First().Message);
			}
		}

		[RequiresSTA]
		public void TestGroupForm_ShouldContainTrackingTabs_AndNotTasksTab()
		{
			using (var form = NewGlbGroupForm(Factory.New<GlbGroup>()))
			{
				form.GroupTabControl.SelectedTab = form.WorkflowTabPage;
				form.Show();

				var workflowControl = form.WorkflowTabPage.FindSingle<ZWorkflowUserControl>();
				var tabNames = workflowControl.MainTabControl.TabPages.Cast<ZTabPage>().Select(t => t.Text);

				AssertSequencesEqual(new[] { "Milestones", "Exceptions", "Triggers", "Events" }, tabNames);
			}
		}

		[RequiresSTA]
		public void TestGroupFormStaffShouldShowWarningsInsteadOfErrors()
		{
			var group = Factory.NewWithValidTestData<GlbGroup>();
			var staff = Factory.NewWithValidTestData<GlbStaff>();
			staff.GS_GB_HomeBranch = ZGuid.Empty;
			staff.GS_GE_HomeDepartment = ZGuid.Empty;
			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_GB_HomeBranch = ZGuid.Empty;
			AssertEquals("Precondition", true, staff.GS_GB_HomeBranchInfo.HasErrors());
			AssertEquals("Precondition", true, staff.GS_GE_HomeDepartmentInfo.HasErrors());
			group.Staff.Add(staff);

			using (var testForm = NewGlbGroupForm(group))
			{
				testForm.Show();
				AssertEquals("Should be set during constructor", true, staff.ShowBranchDepartmentAndPositionErrorsAsWarnings);
				staff.Validation.ValidateAll();
				AssertEquals(false, staff.GS_GB_HomeBranchInfo.HasErrors());
				AssertEquals(true, staff.GS_GB_HomeBranchInfo.HasWarnings());
				AssertEquals(false, staff.GS_GE_HomeDepartmentInfo.HasErrors());
				AssertEquals(true, staff.GS_GE_HomeDepartmentInfo.HasWarnings());

				AssertEquals("Precondition", true, staff2.GS_GB_HomeBranchInfo.HasErrors());
				group.Staff.Add(staff2);
				AssertEquals("Should be set during OnAdded event", true, staff2.ShowBranchDepartmentAndPositionErrorsAsWarnings);
				staff2.Validation.ValidateAll();
				AssertEquals(false, staff2.GS_GB_HomeBranchInfo.HasErrors());
				AssertEquals(true, staff2.GS_GB_HomeBranchInfo.HasWarnings());
			}
		}

		#region Detaching Staff From Group Tests

		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast()
		{
			DetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast(allowed: true, "Do you still want to remove the staff members from this group?", MessageBoxButtons.YesNo);
		}

		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast_AndOperationIsNotAllowed()
		{
			DetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast(allowed: false, "You do not have the relevant permission to make this change.", MessageBoxButtons.OK);
		}

		void DetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast(bool allowed, string message, MessageBoxButtons buttons)
		{
			Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed = allowed;

			// see corresponding test in MasterFiles (TestGetLastStaffInGroupPerGRPCapability)
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "C1";
			group1.GG_IsSecurityEnabled = false;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability2 = Factory.NewWithValidTestData<GlbCapability>();
			capability2.G4_Code = "C2";
			capability2.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability3 = Factory.NewWithValidTestData<GlbCapability>();
			capability3.G4_Code = "C3";
			capability3.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability4 = Factory.NewWithValidTestData<GlbCapability>();
			capability4.G4_Code = "C4";
			capability4.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;
			var capability5 = Factory.NewWithValidTestData<GlbCapability>();
			capability5.G4_Code = "C5";
			capability5.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Capabilities.Add(capability3);
			staff1.Capabilities.Add(capability4);
			staff1.Groups.Add(group1);

			var staff2 = Factory.NewWithValidTestData<GlbStaff>();
			staff2.GS_Code = "S2";
			staff2.Capabilities.Add(capability2);
			staff2.Capabilities.Add(capability3);
			staff2.Capabilities.Add(capability5);
			staff2.Groups.Add(group1);

			var staff3 = Factory.NewWithValidTestData<GlbStaff>();
			staff3.GS_Code = "S3";
			staff3.Capabilities.Add(capability4);
			staff3.Capabilities.Add(capability5);
			staff3.Groups.Add(group1);

			Factory.Save();

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1, staff2 }));

				AssertEquals(@"- Last member(s) in this group with C3 capability: S1, S2.
- Last member(s) in this group with C1 capability: S1.
- Last member(s) in this group with C2 capability: S2.

Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. " + message, groupForm.MembersModuleButtonGrid.DetachMessage.Caption);

				AssertEquals(groupForm.MessageBoxButtons, buttons);
			}
		}

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

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1 }));

				AssertEquals("Are you sure you want to remove the selected staff members from this Group?", groupForm.MembersModuleButtonGrid.DetachMessage.Caption);
				AssertEquals(groupForm.MessageBoxButtons, MessageBoxButtons.YesNo);
			}
		}

		public void TestDetachingStaff_ShouldShowDefaultMessage_WhenStaffIsLast_ButGroupIsSecurityEnabled()
		{
			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1 }));

				AssertEquals("Are you sure you want to remove the selected staff members from this Group?", groupForm.MembersModuleButtonGrid.DetachMessage.Caption);
			}
		}

		[RequiresSTA]
		public void TestDetachingStaff_ShouldShowWarningMessage_WhenStaffIsLast_AndAllGroupsAreConsidered()
		{
			WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.SetValue(Guid.Empty, Guid.Empty, Guid.Empty, false);

			var group1 = Factory.NewWithValidTestData<GlbGroup>();
			group1.GG_Code = "G1";
			group1.GG_IsSecurityEnabled = true;

			var capability1 = Factory.NewWithValidTestData<GlbCapability>();
			capability1.G4_Code = "C1";
			capability1.G4_CapacityScope = GlbCapabilityScopeList.Codes.GroupScope;

			var staff1 = Factory.NewWithValidTestData<GlbStaff>();
			staff1.GS_Code = "S1";
			staff1.Capabilities.Add(capability1);
			staff1.Groups.Add(group1);

			Factory.Save();

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid_BeforeDetaching(null, new ModuleButtonGridBeforeDetachEventArgs(new BusinessObject[] { staff1 }));

				AssertEquals(@"- Last member(s) in this group with C1 capability: S1.

Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this group?", groupForm.MembersModuleButtonGrid.DetachMessage.Caption);
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

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(groupForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				Application.DoEvents();

				AssertEquals(@"- Last member(s) in this group with C1 capability: S1, S2.

Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this group?", UnitTestUserNotification.Instance.LastMessage.Text);
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

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(groupForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.No);
				Application.DoEvents();

				AssertEquals("Staff should not be detached", 2, group1.Staff.Count);
			}
		}

		[RequiresSTA]
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

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements();

				KeySender.PostKeyDown(groupForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();

				AssertEquals("Staff should be detached", 0, group1.Staff.Count);
			}
		}

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

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();
				groupForm.MembersModuleButtonGrid.InnerGrid.SelectAllElements();
				groupForm.MembersModuleButtonGrid.InnerGrid.DeleteMenuItem.PerformClick();

				AssertEquals(@"- Last member(s) in this group with C1 capability: S1.

Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this group?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

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

			using (var groupForm = NewGlbGroupForm(group1))
			{
				groupForm.Show();

				groupForm.MembersModuleButtonGrid.InnerGrid.SelectSingleElementByPK(staff1.PK);
				KeySender.PostKeyDown(groupForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				AssertEquals("Are you sure you want to remove the selected staff members from this Group?", UnitTestUserNotification.Instance.LastMessage.Text);

				groupForm.MembersModuleButtonGrid.InnerGrid.SelectSingleElementByPK(staff2.PK);
				KeySender.PostKeyDown(groupForm.MembersModuleButtonGrid.InnerGrid, Keys.Delete);
				UnitTestUserNotification.Instance.AddAnswer(DialogResult.Yes);
				Application.DoEvents();
				AssertEquals(@"- Last member(s) in this group with C1 capability: S2.

Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this group?", UnitTestUserNotification.Instance.LastMessage.Text);
			}
		}

		#endregion

		#region Implementation

		void AssertSecuredTabPagesEnabledState(GlbGroup group, bool enabled)
		{
			using (var form = NewGlbGroupForm(group))
			{
				form.Show();

				form.GroupTabControl.SelectedTab = form.zStmNoteTabPage1;
				AssertEquals("tabSecurityLabel.Visible", !enabled, form.tabSecurityLabel.Visible);
			}
		}

		protected override Form GetFormToBashCore()
		{
			return NewGlbGroupForm(Factory.New<GlbGroup>());
		}

		GlbGroupForm NewGlbGroupForm(GlbGroup group)
		{
			return new GlbGroupForm(group) { ControllerID = ControllerIDs.GlbGroup };
		}

		#endregion
	}
}
