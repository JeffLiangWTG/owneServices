using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CargoWise.Application;
using CargoWise.Common;
using CargoWise.EntityFramework;
using CargoWise.Glow.CW1.ApplicationCheckpoints.DevIntegration;
using CargoWise.Types;
using CargoWise.Windows.UI;
using CargoWiseOne.ResourceStrings;
using Enterprise.Environment;
using Enterprise.Integration;
using Enterprise.MasterFiles.Business;
using Enterprise.Registry.Business;
using Enterprise.Security;
using Enterprise.Security.ActiveDirectory;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.Environment;
using Enterprise.ZArchitecture.GUI;
using Enterprise.ZArchitecture.Modules;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbGroupForm : ZForm
	{
		public GlbGroupForm()
		{
		}

		public GlbGroupForm(GlbGroup businessEntity) : base(businessEntity)
		{
			ZFormPostingButtonsStrategy.SetupPosting(this, PostingButtonsUserControl);

			if (!this.IsDesignMode())
			{
				GroupTabControl.SelectedIndexChanged += new EventHandler(SecurityTabSetup_OnIndexChanged);

				changeOthersSecurityControl.ChangeOthersSecurity = Group;
				GroupOwnersSecurityControl.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup;
				GroupOwnersSecurityControl.ChangeOthersSecurity = Group;

				SetupSecurityPermissions();
				businessEntity.ChangeActualSecurityPermissions += new EventHandler(RefreshGroupSecurityLabel);
				businessEntity.Staff.AttemptToDeleteFromAllUsers += new EventHandler(DeleteFromAllUsersError);
				businessEntity.Staff.AttemptToDeleteFromSCIM += new EventHandler(DeleteFromSCIMError);
				businessEntity.SecurityPermissions.Security = fGroupSecurity;

				GroupTabControl.PlugIns.AddPlugInAtTabPageIndex(ControllerIDs.GroupCredentialsPlugIn, 1);
				PlugIns.Add(ControllerIDs.eDocsPlugIn);
				PlugIns.Add(ControllerIDs.DocDataPlugIn);

				SetupButtonsSecurity();
				InitialiseActionsMenu();

				this.ADLinkedCheckBox.Visible = !Group.GG_IsSystemDefined && Group.IsADIntegrationEnabled;

				DomainNameDropEdit.Visible = !Group.GG_IsSystemDefined && ((ObjectFactory.Get<IADRegistry>().DomainCredentialsCollection.Any()) && (!EnvProxy.IsHostedWithCargowise || Env.CurrentUser.IsSupportUser));

#if DEBUG
				TypeDescriptor.AddAttributes(SecurityFunctionPathLabel, new SuppressFormsLocalizedTestAttribute());
				TypeDescriptor.AddAttributes(EffectiveSecurityRightsGroupBox, new SuppressFormsLocalizedTestAttribute());
#endif
				businessEntity.Staff.Cast<GlbStaff>().ToList().ForEach(s =>
				{
					s.ShowBranchDepartmentAndPositionErrorsAsWarnings = true;
					s.CurrentGroupLink.Validation.ValidateDomainsForStaffAndGroup();
				});

				businessEntity.Staff.OnStaffAdded += OnStaffAdded;

				MembersModuleButtonGrid.BeforeDetach += MembersModuleButtonGrid_BeforeDetaching;
				MembersModuleButtonGrid.Detached += MembersModuleButtonGrid_OnDetached;
				MembersModuleButtonGrid.InnerGrid.RowsDeleting += MembersModuleButtonGrid_InnerGrid_RowDeleteKeyDown;

				WorkflowTabPage.Initialize(businessEntity);

				if (Group.IsNonSecurityGroup)
				{
					isNonSecurityFirstChecked = true;
				}

				SetupScim();
			}
		}

		protected override void OnShown(EventArgs e)
		{
			base.OnShown(e);
			SetupScim();
		}

		void SetupScim()
		{
			if (Group.IsControlledByScim)
			{
				MembersModuleButtonGrid.SetButtonsReadOnly(true);
				if (!tabSecurityLabel.Visible)
				{
					groupHintLabel.Visible = true;
				}
			}

			if (Group.IsScimMappedGroup)
			{
				MembersModuleButtonGrid.Attaching += MembersModuleButtonGrid_Attaching;
			}
		}

		void MembersModuleButtonGrid_Attaching(object sender, ModuleButtonGridOperationCancelEventArgs args)
		{
			if (BusinessEntity.IsScimMappedGroup && !args.Cancel)
			{
				args.Cancel = true;
				Globals.Message.ShowError(
					Res.GetString("5DC0B491-DF5D-4B4A-AD0C-59C0C2D500C9", "Cannot add staff members to SCIM-mapped groups. Please use relevant mapped checkboxes."),
					Res.GetString("BD229711-0897-46DC-8280-7987B9123601", "Cannot add staff"));
			}
		}

		void OnStaffAdded(object sender, EventArgs e)
		{
			var senderAsStaff = sender as GlbStaff;
			if (senderAsStaff != null && !senderAsStaff.ShowBranchDepartmentAndPositionErrorsAsWarnings)
			{
				senderAsStaff.ShowBranchDepartmentAndPositionErrorsAsWarnings = true;
			}
		}

		void GlbGroupForm_Load(object sender, EventArgs e)
		{
			SetupTabPageSecurity();
		}

		bool isNonSecurityFirstChecked;
		void NonSecurityCheckBox_CheckedChanged(object sender, EventArgs e)
		{
			var newValue = NonSecurityCheckBox.Checked; //hack to get around CheckedChanged being called before underlying property value has changed
			var showConfirm = true;
			if (newValue && isNonSecurityFirstChecked)
			{
				isNonSecurityFirstChecked = false;
				showConfirm = false;
			}
			if (showConfirm)
			{
				if (!Confirm(newValue))
				{
					newValue = !newValue;

					NonSecurityCheckBox.CheckedChanged -= new EventHandler(NonSecurityCheckBox_CheckedChanged);
					NonSecurityCheckBox.Checked = newValue;
					NonSecurityCheckBox.CheckedChanged += new EventHandler(NonSecurityCheckBox_CheckedChanged);
				}
			}
			if (newValue)
			{
				while (!Group.SecurityPermissions.IsValidationSuspended)
				{
					Group.SecurityPermissions.SuspendValidation(); //don't run expensive CalculateHasRightsToChangeOthers validation
				}
			}
			else
			{
				while (Group.SecurityPermissions.IsValidationSuspended)
				{
					Group.SecurityPermissions.ResumeValidation(); //if user ticks and unticks checkbox, don't let user edit security rights without further validation, so resume validation here
				}
			}
			Group.IsNonSecurityGroup = newValue;
			SetupTabPageSecurity();
		}

		bool Confirm(bool isChecked)
		{
			var confirmString = Res.GetString("1A52715A-FB01-4366-8116-7476715C3352", "CONFIRM");
			var confirmCaption = Res.GetString("3002B8D9-2E38-4EEC-B3CB-E7A7E8FEC071", "Confirmation");
			var checkNonSecurityGroupPrompt = Res.GetString("B25D8286-EC0B-4F79-982E-0726AAFDDA1F", "Changing this Group to a Non-Security Group will remove Security Rights previously assigned.");
			var uncheckNonSecurityGroupPrompt = Res.GetString("8B57ED13-CCEB-49C7-94CF-DFC39B43E43A", "When changing a Non-Security Group to a Security Group, previously assigned Security Rights are not restored. Please verify the Security rights that this Group will manage.");

			var message = isChecked ? checkNonSecurityGroupPrompt : uncheckNonSecurityGroupPrompt;
			var answer = Globals.Message.ShowConfirmation(message, confirmCaption, confirmString, ZMessageBoxIcon.Warning, ZMessageBoxButtons.OKCancel);

			return answer == ZDialogResult.OK;
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);

			if (DisplayMode != ODisplayMode.Delete)
			{
				return;
			}

			if (FormVerb.Equals(FormVerbs.Activate))
			{
				((IBusinessObjectState)Group.Staff).DecrementReadOnlyIncludingChildren();
			}
			else if (FormVerb.Equals(FormVerbs.Deactivate) && Group.IsCancelled && Group.Staff.Count == 0)
			{
				MembersHintLabel.Visible = true;
				ControlDpiScalingHelper.SetTop(MembersModuleButtonGrid, MembersModuleButtonGrid.Top + MembersHintLabel.Height, false);
				ControlDpiScalingHelper.SetHeight(MembersModuleButtonGrid, MembersModuleButtonGrid.Height - MembersHintLabel.Height, false);
			}
		}

		void SetupTabPageSecurity()
		{
			var selectedTab = GroupTabControl.SelectedTab;

			if (selectedTab == MembersTabPage)
			{
				if (Group.IsCurrentUserGroupOwnerForThisGroup)
				{
					SetTabPageControls(selectedTab, tabSecurityLabel, true);
				}
				else
				{
					SetTabPageControls(selectedTab, tabSecurityLabel, Env.Security.GroupsViewMembers, Env.Security.GroupsModifyMembers);
				}
			}
			else if (selectedTab == OrganisationsTabPage)
			{
				SetTabPageControls(selectedTab, tabSecurityLabel, Env.Security.GroupsViewOrganisations, Env.Security.GroupsModifyOrganisations);
			}
			else if (selectedTab == SecurityTabPage)
			{
				if (Group.IsNonSecurityGroup)
				{
					SetTabPageControls(selectedTab, tabSecurityLabel, false);
				}
				else
				{
					SetTabPageControls(selectedTab, tabSecurityLabel, Env.Security.GroupsViewSecurityRights, Env.Security.GroupsModifySecurityRights);
				}
			}
			else if (selectedTab == GroupOwnersTabPage)
			{
				SetTabPageControls(selectedTab, tabSecurityLabel, Env.Security.GroupsViewGroupOwners, Env.Security.GroupsModifyGroupOwners);
			}
			else if (selectedTab == zStmNoteTabPage1)
			{
				if (Group.IsCurrentUserGroupOwnerForThisGroup)
				{
					SetTabPageControls(selectedTab, tabSecurityLabel, true);
				}
				else
				{
					SetTabPageControls(selectedTab, tabSecurityLabel, Env.Security.GroupsModify.IsAllowed || Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(Group.PK.ToGuid()).IsAllowed);
				}
			}
			else if (selectedTab == CustomFieldsTabPage)
			{
				SetTabPageControls(selectedTab, tabSecurityLabel, Env.Security.GroupsViewCustomFields, Env.Security.GroupsModifyCustomFields);
			}
			else
			{
				tabSecurityLabel.Visible = false;
			}
		}

		void SetupButtonsSecurity()
		{
			if (!Env.Security.GroupsManageOSMG.IsAllowed)
			{
				OrganisationsModuleButtonGrid.SetButtonsReadOnly(true);
			}
		}

		bool securityLoaded;
		void SecurityTabSetup_OnIndexChanged(object sender, EventArgs e)
		{
			if (!securityLoaded && this.GroupTabControl.SelectedTab.Name.Equals(SecurityTabPage.Name))
			{
				SecurityTreeView.Populate(fGroupSecurity);
				if (ObjectFactory.Get<IAccountingRegistryProvider>().EnablePayablesInvoiceProcessingPortal)
				{
					AddGlowApplicationCheckPoint();
				}
				SecurityGrid.DataSourceChanged += new EventHandler(GlbSecurityBoundGrid_DataSourceChanged);
				securityLoaded = true;
			}
		}

		GlowApplicationCheckpoint glowApplicationCheckPoint;
		void AddGlowApplicationCheckPoint()
		{
			glowApplicationCheckPoint = new GlowApplicationCheckpoint("GlowApplicationCheckpoint", ResString.GetMultilingualString("c63fd22a-4780-475a-a280-fe284b33cf22", "Web Portal Management"), null, fGroupSecurity.SecurityInstance);
			var rootNode = new ZSecurityPointNode(glowApplicationCheckPoint.DisplayText, glowApplicationCheckPoint);

			var childApplicationCheckpoints = ApplicationCheckpoints.GetAll();
			foreach (var childApplicationCheckpoint in childApplicationCheckpoints)
			{
				var childSecurityCheckPoint = new GlowApplicationCheckpoint(childApplicationCheckpoint.Name, (NoResString)childApplicationCheckpoint.Caption, glowApplicationCheckPoint, fGroupSecurity.SecurityInstance);
				var childNode = new ZSecurityPointNode(childSecurityCheckPoint.DisplayText, childSecurityCheckPoint);
				rootNode.Nodes.Add(childNode);
			}
			rootNode.Expand();
			SecurityTreeView.Nodes.Add(rootNode);
		}

		void SetTabPageControls(ZTabPage page, ZLabel label, SecurityCheckpointNonOperationalAllowed viewSecurityCheckpoint, SecurityCheckpointNonOperationalAllowed modifySecurityCheckpoint)
		{
			page.SetupSecurity(viewSecurityCheckpoint);

			SetTabPageControls(page, label, modifySecurityCheckpoint.IsAllowed);
		}

		void SetTabPageControls(TabPage page, ZLabel label, bool controlsEnabled)
		{
			if (page.HasChildren)
			{
				foreach (Control ctrl in page.Controls)
				{
					RecursivelySetControls(ctrl, controlsEnabled);
				}
			}
			else
			{
				//set controls to readonly later when it's bound
				page.BeginInvoke(new Action(() =>
				{
					foreach (Control ctrl in page.Controls)
					{
						RecursivelySetControls(ctrl, controlsEnabled);
					}
				}));
			}

			if (label != null)
			{
				label.Enabled = !controlsEnabled;
				label.Visible = !controlsEnabled;

				if (page == SecurityTabPage && Group.IsNonSecurityGroup)
				{
					label.ForeColor = Color.Red;
					label.Text = Res.GetString("GlbGroupForm|57da4040-c382-40aa-b393-4c297228b10f", "Non-Security Groups can't have security rights added.");
					label.Enabled = true;
					label.Visible = true;
				}
				else if (!controlsEnabled)
				{
					label.ForeColor = Color.Red;
					label.Text = Res.GetString("GlbGroupForm|767a7f81-438f-466b-97d9-e8422a520b8c", "You do not have security rights to edit this tab page.");
					label.Enabled = true;
					label.Visible = true;
				}
				else if (page == MembersTabPage && Group.IsCurrentUserGroupOwnerForThisGroup && !Env.Security.GroupsModifyMembers.IsAllowed)
				{
					label.ForeColor = Color.Blue;
					label.Text = this.IsViewOrDeleteMode ? Res.GetString("GlbGroupForm|08427255-2150-49f1-a90d-e2d25a295c88", "Open this form in Edit mode to modify membership.")
					: Res.GetString("GlbGroupForm|26c203dd-f335-4e70-b1a0-79f8777a2602", "You are a Group Owner and can modify membership.");
					label.Enabled = true;
					label.Visible = true;
				}
				else
				{
					label.Enabled = false;
					label.Visible = false;
				}
			}
		}

		public override string FormCaption => $"{CaptionResourceString.Caption} {Group?.GG_Desc}".Trim();

		#region Initialisation

		void SetupSecurityPermissions()
		{
			using (Group.SuspendSettingHasChanges())
			{
				fGroupSecurity = new SecurityCore(Group.SecurityPermissions, Group, Env.CurrentBranch.PK, Env.CurrentDepartment.PK, Env.CurrentCompany.PK, false) { CachingEnabled = false };
			}
		}

		internal GlbGroup Group => BusinessEntity;

		void InitialiseActionsMenu()
		{
			ZFormMenuStrategy.AddActionsMenuItem(this, GetActionsMenuItems());
		}

		IEnumerable<ZMenuItem> GetActionsMenuItems()
		{
			var adMenuItem = ObjectFactory.Get<IADActionsMenuItemProvider>().GetFormMenuItem(Group);
			if (adMenuItem != null)
			{
				yield return (ZMenuItem)adMenuItem;
				yield return new ZMenuItem("-");
			}

			var resetSecurityPermissionsMenuItem = new ZMenuItem(ResString.GetMultilingualString("MasterFiles.GlbGroupForm.ResetToDefaultGroupRights", "Reset Security Permissions"), ResetSecurityPermissions_Click);
			resetSecurityPermissionsMenuItem.Enabled = Env.Security.GroupsModify.IsAllowed || Env.Security.FindOrCreateChangeGroupSecurityCheckpoint(Group.PK.ToGuid()).IsAllowed;

			yield return resetSecurityPermissionsMenuItem;
		}

		void GlbSecurityBoundGrid_DataSourceChanged(object sender, EventArgs e)
		{
			if (IsHandleCreated && !IsDisposed)
			{
				BeginInvoke(new MethodInvoker(HookIsAllowedColumn));
			}
		}

		void HookIsAllowedColumn()
		{
			var isAllowedColumn = SecurityGrid.Columns[GlbSecuritySchema.GU_SecurityItemIsAllowed.Name];
			if (isAllowedColumn != null)
			{
				((CheckBox)((ZCheckBoxColumnStyle)isAllowedColumn.ColumnStyle).EditControl).CheckedChanged +=
					(sender, e) =>
					{
						SecurityGrid.ListManager.EndCurrentEdit();
						RefreshGroupSecurityLabel();
					};
			}
		}

		#endregion

		#region Delete From All Users Event

		void DeleteFromAllUsersError(object sender, EventArgs e)
		{
			throw new CannotDeleteException("Active staff members cannot be removed from the 'All Users' group.\r\nOnly inactive staff can be removed from this group.");
		}

		#endregion

		#region Delete From SCIM Event

		void DeleteFromSCIMError(object sender, EventArgs e)
		{
			throw new CannotDeleteException("Active staff members cannot be removed from the SCIM-mapped group.");
		}

		#endregion

		#region Detaching Staff From Group

		ResourceStringData GetMessageForLastStaff(IEnumerable<BusinessObject> toDetachBusinessObjects)
		{
			var message = string.Empty;
			var toDetachstaffList = toDetachBusinessObjects;

			if (WorkflowDataRegistry.Instance.CheckOnlyNonSecurityGroupsOnRemovalOfLastStaff.Value && Group.GG_IsSecurityEnabled)
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			var lastStaff = StaffCapabilityGroupHelper.GetLastStaffInGroupPerGRPCapability(Group.Factory, toDetachstaffList.ToArray(), Group)
				.OrderByDescending(c => c.Value.Count()).ThenBy(c => c.Key);

			if (!lastStaff.Any())
			{
				MessageBoxButtons = MessageBoxButtons.YesNo;
				return DefaultDetachMessage;
			}

			foreach (var pair in lastStaff)
			{
				message += Res.GetString("53b338fb-7974-4b2e-bcea-0e6c10a7c04c", "- Last member(s) in this group with {0} capability: {1}.{2}",
					pair.Key, string.Join(", ", pair.Value.OrderBy(s => s)), System.Environment.NewLine);
			}

			message += System.Environment.NewLine;
			var confirmationMessage = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed
				? Res.GetString("7d8fb6c6-02a3-4e96-93fd-1be6c0655826", "Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. Do you still want to remove the staff members from this group?")
				: Res.GetString("CD433209-0B1F-4612-A353-462DE6E52B5D", "Removing the staff members from this group will mean that work relying on the combinations of the group and capability above may get lost. You do not have the relevant permission to make this change.");

			MessageBoxButtons = Env.Security.UserAdminFormsLastObjectRemoval.IsAllowed ? MessageBoxButtons.YesNo : MessageBoxButtons.OK;
			return Res.GetData("1E2C3D08-74F8-405D-955F-818040F30219", "{0}{1}").Format(message, confirmationMessage);
		}

		internal void MembersModuleButtonGrid_BeforeDetaching(object sender, ModuleButtonGridBeforeDetachEventArgs eventArgs)
		{
			var message = GetMessageForLastStaff(eventArgs.ToDetachBusinessObjects);

			if (message != DefaultDetachMessage)
			{
				MembersModuleButtonGrid.DetachMessage = message;
			}
		}

		void MembersModuleButtonGrid_InnerGrid_RowDeleteKeyDown(object sender, RowsDeletingEventArgs e)
		{
			var message = GetMessageForLastStaff(e.Objects);
			var dialogResult = Globals.Message.Show(Res.GetString("ab9c7811-3d5b-43d7-8117-06fd0587be44", "{0}", message.Caption),
				Res.GetString("a8ad6648-5071-4b99-8a85-59e4fb613106", "Confirm Detach..."), MessageBoxButtons, MessageBoxIcon.Information);

			if (dialogResult != DialogResult.Yes)
			{
				e.Cancel = true;
			}
		}

		void MembersModuleButtonGrid_OnDetached(object sender, ModuleButtonGridOnDetachedEventArgs eventArgs)
		{
			MembersModuleButtonGrid.DetachMessage = DefaultDetachMessage;
		}

		ResourceStringData DefaultDetachMessage => Res.GetData("13B5B33F-7709-4D2B-8009-DD6939B6F72E", "Are you sure you want to remove the selected staff members from this Group?");

		internal MessageBoxButtons MessageBoxButtons
		{
			get
			{
				return messageBoxButtons;
			}
			set
			{
				messageBoxButtons = value;
				MembersModuleButtonGrid.MessageBoxButtons = messageBoxButtons;
			}
		}

		MessageBoxButtons messageBoxButtons;

		#endregion

		#region Implementation

		void GroupTabControl_SelectedIndexChanged(object sender, EventArgs e)
		{
			if (GroupTabControl.SelectedTab == SecurityTabPage)
			{
				if (string.IsNullOrEmpty(SecurityBranchGuidFindBox.CurrentCode))
				{
					Group.SecurityBranch = Enterprise.MasterFiles.Business.GlbBranch.CurrentBranch.PK;
				}

				if (string.IsNullOrEmpty(SecurityDepartmentGuidFindBox.CurrentCode))
				{
					Group.SecurityDepartment = Enterprise.MasterFiles.Business.GlbDepartment.CurrentDepartment.PK;
				}
			}

			SetupTabPageSecurity();
		}

		void SetPanelVisible(Control visiblePanel)
		{
			foreach (Control control in splitContainer1.Panel2.Controls.Cast<Control>().ToArray())
			{
				if (control != visiblePanel && control.Visible)
				{
					control.Visible = false;
					control.SendToBack();
				}
			}

			if (visiblePanel != null)
			{
				visiblePanel.Visible = true;
				visiblePanel.BringToFront();
			}
		}

		internal void SecurityTreeView_AfterSelect(object sender, TreeViewEventArgs e)
		{
			ZSecurityPointNode selectedNode = (ZSecurityPointNode)e.Node;
			SecurityTreeView.Focus(); // Hack to make grid lose focus and commit new row.

			if (selectedNode.Checkpoint.LookupKey == glowApplicationCheckPoint?.LookupKey)
			{
				MainSecurityRightsPanel.Visible = false;
			}
			else if (selectedNode.Checkpoint.LookupKey == Env.Security.StaffLocalAdministratorPlaceholder.LookupKey)
			{
				changeOthersSecurityControl.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
				SetPanelVisible(changeOthersSecurityPanel);
				Group.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
				Group.SecurityChangeOthersView.Rebuild();
			}
			else if (selectedNode.Checkpoint.LookupKey == Env.Security.StaffGroupOwnerPlaceholder.LookupKey)
			{
				changeOthersSecurityControl.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
				SetPanelVisible(changeOthersSecurityPanel);
				Group.SecurityChangeOthersView.Mode = GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwner;
				Group.SecurityChangeOthersView.Rebuild();
			}
			else
			{
				SetPanelVisible(MainSecurityRightsPanel);

				if (selectedNode.Checkpoint.LookupKey == Env.Security.AgencyPrincipalAccess.LookupKey ||
					selectedNode.Checkpoint.LookupKey == Env.Security.WhsAllowedClients.LookupKey ||
					selectedNode.Checkpoint.LookupKey == Env.Security.WhsAllowedWarehouses.LookupKey)
				{
					RefreshAllowedOrgsAndWarehousesSecurityPanelData(selectedNode);

					AllowedOrgsAndWarehousesSecurityPanel.Visible = true;
				}
				else
				{
					AllowedOrgsAndWarehousesSecurityPanel.Visible = false;
				}

				SecurityFunctionPathLabel.Text = SecurityTreeView.SelectedNode.Text;
				if (SecurityTreeView.SelectedNode.Parent != null)
				{
					SecurityFunctionPathLabel.Text = SecurityTreeView.SelectedNode.Parent.Text + "|" + SecurityFunctionPathLabel.Text;
				}

				var availableWidth = EffectiveSecurityRightsGroupBox.Width * 2 - ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
				EffectiveSecurityRightsGroupBox.Text = TruncateStringToFit(SecurityFunctionPathLabel.Text, EffectiveSecurityRightsGroupBox.Font, availableWidth);

				RefreshGroupSecurityLabel();
				Group.SecurityPermissionsView.FilterBySecurityKey(selectedNode.Checkpoint.LookupKey, ZGuid.Empty);
			}
		}

		static string TruncateStringToFit(string text, Font font, int avalailableWidth)
		{
			const string truncPostfix = "...";
			if (TextRenderer.MeasureText(text, font).Width > avalailableWidth)
			{
				do
				{
					text = text.Remove(text.Length - 1);
				} while (TextRenderer.MeasureText(text + truncPostfix, font).Width > avalailableWidth);

				text += truncPostfix;
			}

			return text;
		}

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Usage", "CA2222:DoNotDecreaseInheritedMemberVisibility")]
		new GlbGroup BusinessEntity => (GlbGroup)base.BusinessEntity;

		void ResetSecurityPermissions_Click(object sender, EventArgs e)
		{
			BusinessEntity.ResetGroupPermissions();
			RefreshGroupSecurityLabel();
		}

		internal void SecurityGrid_CurrentCellChanged(object sender, EventArgs e)
		{
			RefreshGroupSecurityLabel();
		}

		#region Refresh Allowed Organisations And Warehouses Control

		void RefreshAllowedOrgsAndWarehousesSecurityPanelData(ZSecurityPointNode node)
		{
			AllowedOrgsAndWarehousesSecurityPanel.SetAllowedOrgsAndWarehousesControlData(node.Checkpoint);
			RefreshAllowedOrgsAndWarehousesSecurityGrid(node);
			AllowedOrgsAndWarehousesSecurityPanel.TypeOfFindBoxCollection = Group.SecurityAllowedOrgsAndWarehousesView.FindBoxCollectionType;
		}

		internal void RefreshAllowedOrgsAndWarehousesSecurityGrid(ZSecurityPointNode node)
		{
			if (node.Checkpoint.LookupKey == Env.Security.AgencyPrincipalAccess.LookupKey)
			{
				Group.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedPrincipalsSecurityRightName;
			}
			else if (node.Checkpoint.LookupKey == Env.Security.WhsAllowedClients.LookupKey)
			{
				Group.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedClientsSecurityRightName;
			}
			else if (node.Checkpoint.LookupKey == Env.Security.WhsAllowedWarehouses.LookupKey)
			{
				Group.SecurityAllowedOrgsAndWarehousesView.SecurityRight = GlbSecurity.AllowedWarehousesSecurityRightName;
			}
		}

		#endregion

		void RefreshGroupSecurityLabel(object sender, EventArgs e)
		{
			RefreshGroupSecurityLabel();
		}

		void RefreshGroupSecurityLabel()
		{
			FilterActualSecurityRights();
			if (SecurityTreeView.SelectedNode is ZSecurityPointNode)
			{
				ZSecurityPointNode node = SecurityTreeView.SelectedNode as ZSecurityPointNode;
				GroupSecurityRightLabel.Visible = true;
				if (Group.SecurityBranch == Guid.Empty || Group.SecurityDepartment == Guid.Empty)
				{
					GroupSecurityRightLabel.Text = Res.GetString("GlbGroupForm|NA", "N/A");
				}
				else
				{
					GroupSecurityRightLabel.Text = node.Checkpoint.IsAllowed ? Res.GetString("GlbGroupForm|Yes", "Yes") : Res.GetString("GlbGroupForm|No", "No");
				}
			}
			else
			{
				GroupSecurityRightLabel.Visible = false;
			}
		}

		void FilterActualSecurityRights()
		{
			fGroupSecurity.BranchPK =
				(Group.SecurityBranch != ZGuid.Empty && Group.SecurityBranch != ZGuid.Invalid) ? Group.SecurityBranch.ToGuid() : Guid.Empty;
			fGroupSecurity.CompanyPK =
				(Group.SecurityCompany != ZGuid.Empty && Group.SecurityCompany != ZGuid.Invalid) ? Group.SecurityCompany.ToGuid() : Guid.Empty;
			fGroupSecurity.DepartmentPK =
				(Group.SecurityDepartment != ZGuid.Empty && Group.SecurityDepartment != ZGuid.Invalid) ? Group.SecurityDepartment.ToGuid() : Guid.Empty;
		}

		internal SecurityCore fGroupSecurity;

		internal ZSecurityTreeView SecurityTreeView;

		#endregion

		#region Saving

		protected override ContinueWithSave ShowPreSaveDialogs()
		{
			var result = base.ShowPreSaveDialogs();
			if (result == ContinueWithSave.Yes && Group.IsADIntegrationEnabled && Group.IsADLinked && Group.GG_DescInfo.HasChanges)
			{
				if (Group.IsGroupNameInConflictWithExistingADAccount())
				{
					result = ContinueWithSave.No;
					Globals.Message.ShowError(Res.GetString("6b7631c9-09f0-476a-94d9-1d292ae9fa3b", "Changing this Group Description will result in a conflict with another record in Active Directory. If you wish to link this group with a different record in Active Directory, select Actions > Active Directory > Disconnect from Active Directory first."), Res.GetString("93817634-6666-49e6-9168-cb298c90b96b", "Cannot Save"));
				}
			}
			return result;
		}

		#endregion
	}
}
