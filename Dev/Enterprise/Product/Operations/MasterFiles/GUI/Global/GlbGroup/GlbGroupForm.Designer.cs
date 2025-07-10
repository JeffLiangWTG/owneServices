using System;
using CargoWiseOne.ResourceStrings;

namespace Enterprise.MasterFiles.GUI
{
	partial class GlbGroupForm
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		/// <summary> 
		/// Clean up any resources being used.
		/// </summary>
		/// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				Enterprise.MasterFiles.Business.GlbGroup group = BusinessEntity as Enterprise.MasterFiles.Business.GlbGroup;
				if (group != null)
				{
					group.SecurityPermissions.Security = null;
				}

				if (Group != null)
				{
					Group.Staff.OnStaffAdded -= OnStaffAdded;
					Group.ChangeActualSecurityPermissions -= new EventHandler(RefreshGroupSecurityLabel);
					Group.Staff.AttemptToDeleteFromAllUsers -= new EventHandler(DeleteFromAllUsersError);
					Group.Staff.AttemptToDeleteFromSCIM -= new EventHandler(DeleteFromSCIMError);
					if (MembersModuleButtonGrid != null)
					{
						MembersModuleButtonGrid.Attaching -= MembersModuleButtonGrid_Attaching;
					}
				}

				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		protected sealed override void InitializeComponent()
		{
            this.components = new System.ComponentModel.Container();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
            Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
            this.MembersHintLabel = new Enterprise.ZArchitecture.ZLabel();
            this.GroupTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
            this.MembersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.MembersModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGridWithBlankDetachMessage();
            this.SecurityTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
            this.SecurityTreePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.SecurityTreeView = new Enterprise.MasterFiles.GUI.ZSecurityTreeView();
            this.SecurityFunctionLabel = new Enterprise.ZArchitecture.ZLabel();
            this.MainSecurityRightsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.ActualSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.SecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.EffectiveSecurityRightsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
            this.SecurityGrantedLabel = new Enterprise.ZArchitecture.ZLabel();
            this.SecurityDepartmentGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.SecurityBranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.GroupSecurityRightLabel = new Enterprise.ZArchitecture.ZLabel();
            this.GroupSecurityForLabel = new Enterprise.ZArchitecture.ZLabel();
            this.SecurityFunctionPathLabel = new Enterprise.ZArchitecture.ZLabel();
            this.SecurityGrid = new Enterprise.ZArchitecture.ZGrid();
            this.OrgsAndWarehousesSplitter = new CargoWise.Windows.UI.KSplitter();
            this.AllowedOrgsAndWarehousesSecurityPanel = new Enterprise.MasterFiles.GUI.AllowedOrgsAndWarehousesControl();
            this.changeOthersSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.changeOthersSecurityControl = new Enterprise.MasterFiles.GUI.ChangeOthersSecurityControl();
            this.DynamicSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.CustomFieldsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.customFieldsControl = new Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl();
            this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.GroupOwnersTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.GroupOwnersSecurityPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.GroupOwnersSecurityControl = new Enterprise.MasterFiles.GUI.ChangeOthersSecurityControl();
            this.OrganisationsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
            this.OrganisationsModuleButtonGrid = new Enterprise.ZArchitecture.GUI.ZModuleButtonGrid();
            this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
            this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
            this.ADLinkedCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.DomainNameDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEditWithFixedWidth();
            this.tabSecurityLabel = new Enterprise.ZArchitecture.ZLabel();
			this.groupHintLabel = new Enterprise.ZArchitecture.ZLabel();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.PostingButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
            this.TopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.CategoryDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
            this.GG_IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.NonSecurityCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.GG_DescBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.GG_CodeBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.ParentGroupFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.GroupTabControl.SuspendLayout();
            this.MembersTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MembersModuleButtonGrid.InnerGrid)).BeginInit();
            this.MembersModuleButtonGrid.SuspendLayout();
            this.SecurityTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SecurityTreePanel.SuspendLayout();
            this.MainSecurityRightsPanel.SuspendLayout();
            this.ActualSecurityPanel.SuspendLayout();
            this.SecurityPanel.SuspendLayout();
            this.EffectiveSecurityRightsGroupBox.SuspendLayout();
            this.SecurityDepartmentGuidFindBox.SuspendLayout();
            this.SecurityBranchGuidFindBox.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SecurityGrid)).BeginInit();
            this.SecurityGrid.SuspendLayout();
            this.AllowedOrgsAndWarehousesSecurityPanel.SuspendLayout();
            this.changeOthersSecurityPanel.SuspendLayout();
            this.changeOthersSecurityControl.SuspendLayout();
            this.CustomFieldsTabPage.SuspendLayout();
            this.customFieldsControl.SuspendLayout();
            this.WorkflowTabPage.SuspendLayout();
            this.GroupOwnersTabPage.SuspendLayout();
            this.GroupOwnersSecurityPanel.SuspendLayout();
            this.GroupOwnersSecurityControl.SuspendLayout();
            this.OrganisationsTabPage.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OrganisationsModuleButtonGrid.InnerGrid)).BeginInit();
            this.OrganisationsModuleButtonGrid.SuspendLayout();
            this.zStmNoteTabPage1.SuspendLayout();
            this.DomainNameDropEdit.SuspendLayout();
            this.BottomPanel.SuspendLayout();
            this.PostingButtonsUserControl.SuspendLayout();
            this.TopPanel.SuspendLayout();
            this.CategoryDropEdit.SuspendLayout();
            this.ParentGroupFindBox.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 550, true);
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 24, true);
            this.MainStatusBar.SizingGrip = false;
            // 
            // MessageStatusBarPanel
            // 
            this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(703);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbGroup);
            // 
            // MembersHintLabel
            // 
            this.MembersHintLabel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MembersHintLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbStaffForm|5AF6FEBB-5EF2-4266-97F1-B06527D8BCA8", "All members have been detached from the group.");
            this.MembersHintLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.MembersHintLabel.ForeColor = System.Drawing.Color.Red;
            this.MembersHintLabel.IsFontBold = true;
            this.MembersHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MembersHintLabel.Name = "MembersHintLabel";
            this.MembersHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(827, 16, true);
            this.MembersHintLabel.TabIndex = 0;
            this.MembersHintLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            this.MembersHintLabel.Visible = false;
            // 
            // GroupTabControl
            // 
            this.GroupTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
            this.GroupTabControl.Controls.Add(this.MembersTabPage);
            this.GroupTabControl.Controls.Add(this.SecurityTabPage);
            this.GroupTabControl.Controls.Add(this.CustomFieldsTabPage);
            this.GroupTabControl.Controls.Add(this.WorkflowTabPage);
            this.GroupTabControl.Controls.Add(this.GroupOwnersTabPage);
            this.GroupTabControl.Controls.Add(this.OrganisationsTabPage);
            this.GroupTabControl.Controls.Add(this.zStmNoteTabPage1);
            this.GroupTabControl.Controls.Add(this.zLogsTabPage1);
            this.GroupTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GroupTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
            this.GroupTabControl.Name = "GroupTabControl";
            this.GroupTabControl.SelectedIndex = 0;
            this.GroupTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 426, true);
            this.GroupTabControl.TabIndex = 1;
            this.GroupTabControl.SelectedIndexChanged += new System.EventHandler(this.GroupTabControl_SelectedIndexChanged);
            // 
            // MembersTabPage
            // 
            this.MembersTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|c5c6acbd-e01d-43ae-b9e3-046b38f9f6e9", "Members");
            this.MembersTabPage.Controls.Add(this.MembersHintLabel);
            this.MembersTabPage.Controls.Add(this.MembersModuleButtonGrid);
            this.MembersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.MembersTabPage.Name = "MembersTabPage";
            this.MembersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.MembersTabPage.TabIndex = 0;
            // 
            // MembersModuleButtonGrid
            // 
            this.MembersModuleButtonGrid.AllowAttachDetachWithoutEditSecurity = true;
            this.MembersModuleButtonGrid.AllowDrop = true;
            this.MembersModuleButtonGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.MembersModuleButtonGrid, "Staff");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).Staff)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).Lookups.CompleteStaffList)));
            this.MembersModuleButtonGrid.BindToFindBoxList = "Lookups+CompleteStaffList";
            zTextBoxColumnStyleInfo1.ColumnName = "GS_LoginName";
            zTextBoxColumnStyleInfo1.IsMandatory = true;
            zTextBoxColumnStyleInfo1.IsReadOnly = true;
            zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo2.ColumnName = "GS_FullName";
            zTextBoxColumnStyleInfo2.IsMandatory = true;
            zTextBoxColumnStyleInfo2.IsReadOnly = true;
            zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("926ccb2a-2261-407f-9304-062dbfc48290", "Title");
            zTextBoxColumnStyleInfo3.ColumnName = "GS_Title";
            zTextBoxColumnStyleInfo3.IsReadOnly = true;
            zTextBoxColumnStyleInfo3.IsVisible = false;
            zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo4.ColumnName = "HomeDepartment+GE_Desc";
            zTextBoxColumnStyleInfo4.IsReadOnly = true;
            zTextBoxColumnStyleInfo4.IsVisible = false;
            zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
            zTextBoxColumnStyleInfo5.ColumnName = "HomeBranch+GB_BranchName";
            zTextBoxColumnStyleInfo5.IsReadOnly = true;
            zTextBoxColumnStyleInfo5.IsVisible = false;
            zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo6.ColumnName = "HomeBranch+Company+GC_Name";
            zTextBoxColumnStyleInfo6.IsReadOnly = true;
            zTextBoxColumnStyleInfo6.IsVisible = false;
            zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            zTextBoxColumnStyleInfo7.ColumnName = "GS_Code";
            zTextBoxColumnStyleInfo7.IsReadOnly = true;
            zTextBoxColumnStyleInfo7.IsVisible = false;
            zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo8.ColumnName = "GS_DomainName";
            zTextBoxColumnStyleInfo8.IsReadOnly = true;
            zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zDropEditColumnStyleInfo1.ColumnName = "CurrentGroupLink+GK_MembershipType";
            zDropEditColumnStyleInfo1.ToolTip = "The type of member the staff is in this group.";
            zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
            zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "";
            zCalcEditColumnStyleInfo1.ColumnName = "CurrentGroupLink+GK_SkillLevel";
            zCalcEditColumnStyleInfo1.Decimals = 0;
            zCalcEditColumnStyleInfo1.ToolTip = "The Staff member\'s skill level in this group as a number between 0 and 10";
            zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
            this.MembersModuleButtonGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
            this.MembersModuleButtonGrid.DetachMessage = Enterprise.MasterFiles.GUI.Res.GetData("13B5B33F-7709-4D2B-8009-DD6939B6F72E", "Are you sure you want to remove the selected staff members from this Group?");
            this.MembersModuleButtonGrid.GridId = null;
            // 
            // 
            // 
            this.MembersModuleButtonGrid.InnerGrid.AllowNavigation = false;
            this.MembersModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.MembersModuleButtonGrid.InnerGrid.CaptionVisible = false;
            this.MembersModuleButtonGrid.InnerGrid.GridId = null;
            this.MembersModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.MembersModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
            this.MembersModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.MembersModuleButtonGrid.InnerGrid.Name = "Grid";
            this.MembersModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
            this.MembersModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 361, true);
            this.MembersModuleButtonGrid.InnerGrid.TabIndex = 0;
            this.MembersModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MembersModuleButtonGrid.MessageBoxButtons = System.Windows.Forms.MessageBoxButtons.OK;
            this.MembersModuleButtonGrid.ModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.GlbStaff;
            this.MembersModuleButtonGrid.Name = "MembersModuleButtonGrid";
            this.MembersModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("7E9B95C9-6E05-40A0-A757-C1C058456733", "Staff");
            this.MembersModuleButtonGrid.ReadOnly = false;
            this.MembersModuleButtonGrid.ShowEditButton = false;
            this.MembersModuleButtonGrid.ShowNewButton = false;
            this.MembersModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.MembersModuleButtonGrid.TabIndex = 1;
            // 
            // SecurityTabPage
            // 
            this.SecurityTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|82b29fff-8b40-4946-95ed-f1ed82e3e0a2", "Security");
            this.SecurityTabPage.Controls.Add(this.splitContainer1);
            this.SecurityTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.SecurityTabPage.Name = "SecurityTabPage";
            this.SecurityTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.SecurityTabPage.TabIndex = 1;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.SecurityTreePanel);
            this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.splitContainer1.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.MainSecurityRightsPanel);
            this.splitContainer1.Panel2.Controls.Add(this.changeOthersSecurityPanel);
            this.splitContainer1.Panel2.Controls.Add(this.DynamicSecurityPanel);
            this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(230);
            this.splitContainer1.TabIndex = 24;
            // 
            // SecurityTreePanel
            // 
            this.SecurityTreePanel.Controls.Add(this.SecurityTreeView);
            this.SecurityTreePanel.Controls.Add(this.SecurityFunctionLabel);
            this.SecurityTreePanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.SecurityTreePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SecurityTreePanel.Name = "SecurityTreePanel";
            this.SecurityTreePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 399, true);
            this.SecurityTreePanel.TabIndex = 22;
            // 
            // SecurityTreeView
            // 
            this.SecurityTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SecurityTreeView.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawText;
            this.SecurityTreeView.HideSelection = false;
            this.SecurityTreeView.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 16, true);
            this.SecurityTreeView.Name = "SecurityTreeView";
            this.SecurityTreeView.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 383, true);
            this.SecurityTreeView.TabIndex = 0;
            this.SecurityTreeView.TreeViewSearcher = null;
            this.SecurityTreeView.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.SecurityTreeView_AfterSelect);
            // 
            // SecurityFunctionLabel
            // 
            this.SecurityFunctionLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|f38b4feb-558a-4605-9b74-068f0dc72b2f", "Select Security Function");
            this.SecurityFunctionLabel.Dock = System.Windows.Forms.DockStyle.Top;
            this.SecurityFunctionLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SecurityFunctionLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.SecurityFunctionLabel.Name = "SecurityFunctionLabel";
            this.SecurityFunctionLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(230, 16, true);
            this.SecurityFunctionLabel.TabIndex = 19;
            // 
            // MainSecurityRightsPanel
            // 
            this.MainSecurityRightsPanel.Controls.Add(this.ActualSecurityPanel);
            this.MainSecurityRightsPanel.Controls.Add(this.OrgsAndWarehousesSplitter);
            this.MainSecurityRightsPanel.Controls.Add(this.AllowedOrgsAndWarehousesSecurityPanel);
            this.MainSecurityRightsPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.MainSecurityRightsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.MainSecurityRightsPanel.Name = "MainSecurityRightsPanel";
            this.MainSecurityRightsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 399, true);
            this.MainSecurityRightsPanel.TabIndex = 23;
            // 
            // ActualSecurityPanel
            // 
            this.ActualSecurityPanel.Controls.Add(this.SecurityPanel);
            this.ActualSecurityPanel.Controls.Add(this.SecurityGrid);
            this.ActualSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ActualSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ActualSecurityPanel.Name = "ActualSecurityPanel";
            this.ActualSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 214, true);
            this.ActualSecurityPanel.TabIndex = 22;
            // 
            // SecurityPanel
            // 
            this.SecurityPanel.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.SecurityPanel.Controls.Add(this.EffectiveSecurityRightsGroupBox);
            this.SecurityPanel.Controls.Add(this.SecurityFunctionPathLabel);
            this.SecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 8, true);
            this.SecurityPanel.Name = "SecurityPanel";
            this.SecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 84, true);
            this.SecurityPanel.TabIndex = 1;
            // 
            // EffectiveSecurityRightsGroupBox
            // 
            this.EffectiveSecurityRightsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|43758083-bde8-400c-a0e7-044059413964", "Effective Security Right for:");
            this.EffectiveSecurityRightsGroupBox.Controls.Add(this.SecurityGrantedLabel);
            this.EffectiveSecurityRightsGroupBox.Controls.Add(this.SecurityDepartmentGuidFindBox);
            this.EffectiveSecurityRightsGroupBox.Controls.Add(this.SecurityBranchGuidFindBox);
            this.EffectiveSecurityRightsGroupBox.Controls.Add(this.GroupSecurityRightLabel);
            this.EffectiveSecurityRightsGroupBox.Controls.Add(this.GroupSecurityForLabel);
            this.EffectiveSecurityRightsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 2, true);
            this.EffectiveSecurityRightsGroupBox.Name = "EffectiveSecurityRightsGroupBox";
            this.EffectiveSecurityRightsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(428, 79, true);
            this.EffectiveSecurityRightsGroupBox.TabIndex = 19;
            this.EffectiveSecurityRightsGroupBox.TabStop = false;
            // 
            // SecurityGrantedLabel
            // 
            this.SecurityGrantedLabel.AutoSize = true;
            this.SecurityGrantedLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|5089575a-fdc1-40cc-b980-e13249d1ef9d", "Granted");
            this.SecurityGrantedLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.SecurityGrantedLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.SecurityGrantedLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(352, 16, true);
            this.SecurityGrantedLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 45, true);
            this.SecurityGrantedLabel.Name = "SecurityGrantedLabel";
            this.SecurityGrantedLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 32, 0, 0, true);
            this.SecurityGrantedLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(46, 45, true);
            this.SecurityGrantedLabel.TabIndex = 11;
            this.SecurityGrantedLabel.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // SecurityDepartmentGuidFindBox
            // 
            this.SecurityDepartmentGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SecurityDepartmentGuidFindBox, "SecurityDepartment");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityDepartment)));
            this.SecurityDepartmentGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|921b6a76-2576-4818-84ee-710d9c03c8af", "Department");
            this.SecurityDepartmentGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(224, 46, true);
            this.SecurityDepartmentGuidFindBox.Name = "SecurityDepartmentGuidFindBox";
            this.SecurityDepartmentGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.SecurityDepartmentGuidFindBox.ParentType = null;
            this.SecurityDepartmentGuidFindBox.ShowDescriptionBox = false;
            this.SecurityDepartmentGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.SecurityDepartmentGuidFindBox.TabIndex = 13;
            // 
            // SecurityBranchGuidFindBox
            // 
            this.SecurityBranchGuidFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.SecurityBranchGuidFindBox, "SecurityBranch");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityBranch)));
            this.SecurityBranchGuidFindBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|21bfa1ad-66d4-4071-9f83-c917b999a072", "Branch");
            this.SecurityBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(52, 46, true);
            this.SecurityBranchGuidFindBox.Name = "SecurityBranchGuidFindBox";
            this.SecurityBranchGuidFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.SecurityBranchGuidFindBox.ParentType = null;
            this.SecurityBranchGuidFindBox.ShowDescriptionBox = false;
            this.SecurityBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(96, 20, true);
            this.SecurityBranchGuidFindBox.TabIndex = 12;
            // 
            // GroupSecurityRightLabel
            // 
            this.GroupSecurityRightLabel.AutoSize = true;
            this.GroupSecurityRightLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|1c7bebf7-432a-4249-91b6-07b3a37154fb", "Yes");
            this.GroupSecurityRightLabel.Dock = System.Windows.Forms.DockStyle.Right;
            this.GroupSecurityRightLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.GroupSecurityRightLabel.IsFontBold = true;
            this.GroupSecurityRightLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(398, 16, true);
            this.GroupSecurityRightLabel.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 45, true);
            this.GroupSecurityRightLabel.Name = "GroupSecurityRightLabel";
            this.GroupSecurityRightLabel.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(0, 32, 0, 0, true);
            this.GroupSecurityRightLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(27, 45, true);
            this.GroupSecurityRightLabel.TabIndex = 10;
            // 
            // GroupSecurityForLabel
            // 
            this.GroupSecurityForLabel.AutoSize = true;
            this.GroupSecurityForLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|1bd7f953-8b33-433e-8d3f-82a9f1147724", "Check Group Security for");
            this.GroupSecurityForLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif)));
            this.GroupSecurityForLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 4, true);
            this.GroupSecurityForLabel.Name = "GroupSecurityForLabel";
            this.GroupSecurityForLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.GroupSecurityForLabel.TabIndex = 0;
            this.GroupSecurityForLabel.Visible = false;
            // 
            // SecurityFunctionPathLabel
            // 
            this.SecurityFunctionPathLabel.AutoSize = true;
            this.SecurityFunctionPathLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|47b7e44d-7b65-49a4-bae6-bc454a2fc09d", "Security Function");
            this.SecurityFunctionPathLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.SecurityFunctionPathLabel.IsFontBold = true;
            this.SecurityFunctionPathLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(136, 4, true);
            this.SecurityFunctionPathLabel.Name = "SecurityFunctionPathLabel";
            this.SecurityFunctionPathLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
            this.SecurityFunctionPathLabel.TabIndex = 7;
            this.SecurityFunctionPathLabel.Visible = false;
            // 
            // SecurityGrid
            // 
            this.SecurityGrid.AllowNavigation = false;
            this.SecurityGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.BindingSource.SetBindingMember(this.SecurityGrid, "SecurityPermissionsView");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityPermissionsView)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityPermissionsView)).SyncRoot)).CompanyCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityPermissionsView)).SyncRoot)).BranchCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityPermissionsView)).SyncRoot)).DepartmentCode)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbSecurity)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityPermissionsView)).SyncRoot)).GU_SecurityItemIsAllowed)));
            this.SecurityGrid.CaptionVisible = false;
            zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|94fe4f8e-293b-4236-af40-cbbc640f9593", "Company");
            zCodeFindBoxColumnStyleInfo1.ColumnName = "CompanyCode";
            zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|225188d7-bb9c-4ce5-b98a-761db84a3d75", "Branch");
            zCodeFindBoxColumnStyleInfo2.ColumnName = "BranchCode";
            zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|e056d98e-8151-46f2-9b93-99fe1a4664fe", "Department");
            zCodeFindBoxColumnStyleInfo3.ColumnName = "DepartmentCode";
            zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            zCheckBoxColumnStyleInfo1.ColumnName = "GU_SecurityItemIsAllowed";
            zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
            this.SecurityGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
            this.SecurityGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
            this.SecurityGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
            this.SecurityGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
            this.SecurityGrid.GridId = "3d58d677-727a-48b2-8c3f-bd77095d3485";
            this.SecurityGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.SecurityGrid.LayoutKey = "SecurityGrid";
            this.SecurityGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 98, true);
            this.SecurityGrid.Name = "SecurityGrid";
            this.SecurityGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(635, 108, true);
            this.SecurityGrid.TabIndex = 2;
            this.SecurityGrid.CurrentCellChanged += new System.EventHandler(this.SecurityGrid_CurrentCellChanged);
            // 
            // OrgsAndWarehousesSplitter
            // 
            this.OrgsAndWarehousesSplitter.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.OrgsAndWarehousesSplitter.DoNotSaveSplitterLayout = false;
            this.OrgsAndWarehousesSplitter.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 214, true);
            this.OrgsAndWarehousesSplitter.MinExtra = 150;
            this.OrgsAndWarehousesSplitter.MinSize = 150;
            this.OrgsAndWarehousesSplitter.Name = "OrgsAndWarehousesSplitter";
            this.OrgsAndWarehousesSplitter.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 3, true);
            this.OrgsAndWarehousesSplitter.TabIndex = 24;
            this.OrgsAndWarehousesSplitter.TabStop = false;
            // 
            // AllowedOrgsAndWarehousesSecurityPanel
            // 
            this.AllowedOrgsAndWarehousesSecurityPanel.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.AllowedOrgsAndWarehousesSecurityPanel, "SecurityAllowedOrgsAndWarehousesView");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbSecurityAllowedOrgsAndWarehousesView)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).SecurityAllowedOrgsAndWarehousesView)));
            this.AllowedOrgsAndWarehousesSecurityPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.AllowedOrgsAndWarehousesSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 217, true);
            this.AllowedOrgsAndWarehousesSecurityPanel.Name = "AllowedOrgsAndWarehousesSecurityPanel";
            this.AllowedOrgsAndWarehousesSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 182, true);
            this.AllowedOrgsAndWarehousesSecurityPanel.TabIndex = 23;
            this.AllowedOrgsAndWarehousesSecurityPanel.TypeOfFindBoxCollection = typeof(Enterprise.MasterFiles.Business.ShipsAgencyPrincipalCollection);
            // 
            // changeOthersSecurityPanel
            // 
            this.changeOthersSecurityPanel.Controls.Add(this.changeOthersSecurityControl);
            this.changeOthersSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changeOthersSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.changeOthersSecurityPanel.Name = "changeOthersSecurityPanel";
            this.changeOthersSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 399, true);
            this.changeOthersSecurityPanel.TabIndex = 22;
            // 
            // changeOthersSecurityControl
            // 
            this.changeOthersSecurityControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.changeOthersSecurityControl, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(((Enterprise.MasterFiles.Business.GlbGroup)(null)))));
            this.changeOthersSecurityControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.changeOthersSecurityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.changeOthersSecurityControl.Mode = Enterprise.MasterFiles.Business.GlbSecurityChangeOthersView.ChangeOthersMode.LocalAdministrator;
            this.changeOthersSecurityControl.Name = "changeOthersSecurityControl";
            this.changeOthersSecurityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 399, true);
            this.changeOthersSecurityControl.TabIndex = 0;
            // 
            // DynamicSecurityPanel
            // 
            this.DynamicSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.DynamicSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.DynamicSecurityPanel.Name = "DynamicSecurityPanel";
            this.DynamicSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(642, 399, true);
            this.DynamicSecurityPanel.TabIndex = 24;
            // 
            // CustomFieldsTabPage
            // 
            this.CustomFieldsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|fcd4161c-7efa-4464-91bd-5f639e9cf8e0", "Custom Fields");
            this.CustomFieldsTabPage.Controls.Add(this.customFieldsControl);
            this.CustomFieldsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.CustomFieldsTabPage.Name = "CustomFieldsTabPage";
            this.CustomFieldsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.CustomFieldsTabPage.TabIndex = 2;
            // 
            // customFieldsControl
            // 
            this.customFieldsControl.AllowDrop = true;
            this.customFieldsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.customFieldsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.customFieldsControl.Name = "customFieldsControl";
            this.customFieldsControl.NothingSetupMessageLabelText = Res.GetString("4f150cbb-fdc9-4031-87e0-b3df76ccc4e1", "To make use of this tab, please setup Groups custom fields in Workflow Manager.");
            this.customFieldsControl.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(5, true);
            this.customFieldsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.customFieldsControl.TabIndex = 0;
            // 
            // WorkflowTabPage
            // 
            this.WorkflowTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|cbb889d3-3e4a-47fa-bafc-643a83b46db1", "Workflow & Tracking");
            this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.WorkflowTabPage.Name = "WorkflowTabPage";
            this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.WorkflowTabPage.TabIndex = 3;
            // 
            // GroupOwnersTabPage
            // 
            this.GroupOwnersTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|daa453d8-089f-4852-ae0b-ad46692056fe", "Group Owners");
            this.GroupOwnersTabPage.Controls.Add(this.GroupOwnersSecurityPanel);
            this.GroupOwnersTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.GroupOwnersTabPage.Name = "GroupOwnersTabPage";
            this.GroupOwnersTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.GroupOwnersTabPage.TabIndex = 2;
            // 
            // GroupOwnersSecurityPanel
            // 
            this.GroupOwnersSecurityPanel.Controls.Add(this.GroupOwnersSecurityControl);
            this.GroupOwnersSecurityPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GroupOwnersSecurityPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GroupOwnersSecurityPanel.Name = "GroupOwnersSecurityPanel";
            this.GroupOwnersSecurityPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.GroupOwnersSecurityPanel.TabIndex = 22;
            // 
            // GroupOwnersSecurityControl
            // 
            this.GroupOwnersSecurityControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.GroupOwnersSecurityControl, ".");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.ISupportChangeOthersSecurity)(((Enterprise.MasterFiles.Business.GlbGroup)(null)))));
            this.GroupOwnersSecurityControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.GroupOwnersSecurityControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.GroupOwnersSecurityControl.Mode = Enterprise.MasterFiles.Business.GlbSecurityChangeOthersView.ChangeOthersMode.GroupOwnersForGroup;
            this.GroupOwnersSecurityControl.Name = "GroupOwnersSecurityControl";
            this.GroupOwnersSecurityControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.GroupOwnersSecurityControl.TabIndex = 0;
            // 
            // OrganisationsTabPage
            // 
            this.OrganisationsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|03758c78-1ea3-4455-a822-ccce6b72c1a8", "Organizations");
            this.OrganisationsTabPage.Controls.Add(this.OrganisationsModuleButtonGrid);
            this.OrganisationsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.OrganisationsTabPage.Name = "OrganisationsTabPage";
            this.OrganisationsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.OrganisationsTabPage.TabIndex = 0;
            // 
            // OrganisationsModuleButtonGrid
            // 
            this.OrganisationsModuleButtonGrid.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.OrganisationsModuleButtonGrid, "Organisation");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).Organisation)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).Lookups.CompleteOrganisationList)));
            this.OrganisationsModuleButtonGrid.BindToFindBoxList = "Lookups+CompleteOrganisationList";
            zTextBoxColumnStyleInfo9.ColumnName = "OH_Code";
            zTextBoxColumnStyleInfo9.IsMandatory = true;
            zTextBoxColumnStyleInfo9.IsReadOnly = true;
            zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
            zTextBoxColumnStyleInfo10.ColumnName = "OH_FullName";
            zTextBoxColumnStyleInfo10.IsMandatory = true;
            zTextBoxColumnStyleInfo10.IsReadOnly = true;
            zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
            this.OrganisationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
            this.OrganisationsModuleButtonGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
            this.OrganisationsModuleButtonGrid.DetachMessage = Enterprise.MasterFiles.GUI.Res.GetData("03c70934-3549-415e-81c5-80762607feb5", "Are you sure you want to remove the selected organization from this Group?");
            this.OrganisationsModuleButtonGrid.Dock = System.Windows.Forms.DockStyle.Fill;
            this.OrganisationsModuleButtonGrid.GridId = null;
            // 
            // 
            // 
            this.OrganisationsModuleButtonGrid.InnerGrid.AllowNavigation = false;
            this.OrganisationsModuleButtonGrid.InnerGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.OrganisationsModuleButtonGrid.InnerGrid.CaptionVisible = false;
            this.OrganisationsModuleButtonGrid.InnerGrid.GridId = null;
            this.OrganisationsModuleButtonGrid.InnerGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
            this.OrganisationsModuleButtonGrid.InnerGrid.LayoutKey = "Grid";
            this.OrganisationsModuleButtonGrid.InnerGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
            this.OrganisationsModuleButtonGrid.InnerGrid.Name = "Grid";
            this.OrganisationsModuleButtonGrid.InnerGrid.RemoveAction = Enterprise.ZArchitecture.RemoveAction.Remove;
            this.OrganisationsModuleButtonGrid.InnerGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(870, 361, true);
            this.OrganisationsModuleButtonGrid.InnerGrid.TabIndex = 0;
            this.OrganisationsModuleButtonGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.OrganisationsModuleButtonGrid.ModuleID = ((Enterprise.ZArchitecture.Modules.OrgModuleIdentifier)(Enterprise.ZArchitecture.Modules.ModuleIDs.Organisation));
            this.OrganisationsModuleButtonGrid.Name = "OrganisationsModuleButtonGrid";
            this.OrganisationsModuleButtonGrid.NameOfAGridElement = Enterprise.MasterFiles.GUI.Res.GetData("03d21f91-9a9a-4606-8181-b1e0cfeff890", "Organization");
            this.OrganisationsModuleButtonGrid.ReadOnly = false;
            this.OrganisationsModuleButtonGrid.ShowEditButton = false;
            this.OrganisationsModuleButtonGrid.ShowNewButton = false;
            this.OrganisationsModuleButtonGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.OrganisationsModuleButtonGrid.TabIndex = 0;
            // 
            // zStmNoteTabPage1
            // 
            this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
            this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.zStmNoteTabPage1.TabIndex = 2;
            // 
            // zLogsTabPage1
            // 
            this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
            this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.zLogsTabPage1.Name = "zLogsTabPage1";
            this.zLogsTabPage1.ShouldBeReadOnlyInViewMode = false;
            this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(876, 399, true);
            this.zLogsTabPage1.TabIndex = 3;
            // 
            // ADLinkedCheckBox
            // 
            this.BindingSource.SetBindingMember(this.ADLinkedCheckBox, "IsADLinked");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).IsADLinked)));
            this.ADLinkedCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.ADLinkedCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.ADLinkedCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(448, 9, true);
            this.ADLinkedCheckBox.Name = "ADLinkedCheckBox";
            this.ADLinkedCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 24, true);
            this.ADLinkedCheckBox.TabIndex = 14;
            this.ADLinkedCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // DomainNameDropEdit
            // 
            this.DomainNameDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.DomainNameDropEdit, "DomainName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).DomainName)));
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).DomainName)));
            this.DomainNameDropEdit.BindToForDescription = "DomainName";
            this.DomainNameDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Lower;
            this.DomainNameDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 35, true);
            this.DomainNameDropEdit.Name = "DomainNameDropEdit";
            this.DomainNameDropEdit.ShouldResizeByMaxLength = true;
            this.DomainNameDropEdit.ShowDescriptionBox = false;
            this.DomainNameDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
            this.DomainNameDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
            this.DomainNameDropEdit.TabIndex = 15;
            this.DomainNameDropEdit.UseFullWidthForCodeBox = true;
            // 
            // tabSecurityLabel
            // 
            this.tabSecurityLabel.AutoSize = true;
            this.tabSecurityLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|767a7f81-438f-466b-97d9-e8422a520b8c", "You do not have security rights to edit this tab page.");
            this.tabSecurityLabel.FontType = ((Enterprise.ZArchitecture.Core.OFontTypes)((Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.Bold)));
            this.tabSecurityLabel.ForeColor = System.Drawing.Color.Red;
            this.tabSecurityLabel.IsFontBold = true;
            this.tabSecurityLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 14, true);
            this.tabSecurityLabel.Name = "tabSecurityLabel";
            this.tabSecurityLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 13, true);
            this.tabSecurityLabel.TabIndex = 1;
			// 
			// groupHintLabel
			// 
			this.groupHintLabel.AutoSize = true;
			this.groupHintLabel.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|2ACC243C-6762-4135-A978-D7F284106BAB", "Some fields cannot be edited because they are controlled externally.");
			this.groupHintLabel.ForeColor = System.Drawing.Color.Red;
			this.groupHintLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(569, 14, true);
			this.groupHintLabel.Name = "groupHintLabel";
			this.groupHintLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(303, 13, true);
			this.groupHintLabel.TabIndex = 1;
			this.groupHintLabel.Visible = false;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.PostingButtonsUserControl);
            this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 514, true);
            this.BottomPanel.Name = "BottomPanel";
            this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 36, true);
            this.BottomPanel.TabIndex = 2;
            // 
            // PostingButtonsUserControl
            // 
            this.PostingButtonsUserControl.AllowDrop = true;
            this.PostingButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.PostingButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(575, 8, true);
            this.PostingButtonsUserControl.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(241, 25, true);
            this.PostingButtonsUserControl.Name = "PostingButtonsUserControl";
            this.PostingButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(302, 25, true);
            this.PostingButtonsUserControl.TabIndex = 0;
            // 
            // TopPanel
            // 
            this.TopPanel.Controls.Add(this.DomainNameDropEdit);
            this.TopPanel.Controls.Add(this.ADLinkedCheckBox);
            this.TopPanel.Controls.Add(this.CategoryDropEdit);
            this.TopPanel.Controls.Add(this.GG_IsActiveCheckBox);
            this.TopPanel.Controls.Add(this.tabSecurityLabel);
			this.TopPanel.Controls.Add(this.groupHintLabel);
			this.TopPanel.Controls.Add(this.NonSecurityCheckBox);
            this.TopPanel.Controls.Add(this.GG_DescBoundTextBox);
            this.TopPanel.Controls.Add(this.GG_CodeBoundTextBox);
            this.TopPanel.Controls.Add(this.ParentGroupFindBox);
            this.TopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.TopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.TopPanel.Name = "TopPanel";
            this.TopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 88, true);
            this.TopPanel.TabIndex = 0;
            // 
            // CategoryDropEdit
            // 
            this.CategoryDropEdit.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.CategoryDropEdit, "GG_Category");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).GG_Category)));
            this.CategoryDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 61, true);
            this.CategoryDropEdit.Name = "CategoryDropEdit";
            this.CategoryDropEdit.PreBoundMaxLength = 3;
            this.CategoryDropEdit.ShouldResizeByMaxLength = true;
            this.CategoryDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
            this.CategoryDropEdit.TabIndex = 16;
            // 
            // GG_IsActiveCheckBox
            // 
            this.BindingSource.SetBindingMember(this.GG_IsActiveCheckBox, "GG_IsActive");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).GG_IsActive)));
            this.GG_IsActiveCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.GG_IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.GG_IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(194, 9, true);
            this.GG_IsActiveCheckBox.Name = "GG_IsActiveCheckBox";
            this.GG_IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 24, true);
            this.GG_IsActiveCheckBox.TabIndex = 12;
            this.GG_IsActiveCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // NonSecurityCheckBox
            // 
            this.BindingSource.SetBindingMember(this.NonSecurityCheckBox, "IsNonSecurityGroup");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).IsNonSecurityGroup)));
            this.NonSecurityCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
            this.NonSecurityCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
            this.NonSecurityCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(309, 9, true);
            this.NonSecurityCheckBox.Name = "NonSecurityCheckBox";
            this.NonSecurityCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 24, true);
            this.NonSecurityCheckBox.TabIndex = 13;
            this.NonSecurityCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.NonSecurityCheckBox.CheckedChanged += new System.EventHandler(this.NonSecurityCheckBox_CheckedChanged);
            // 
            // GG_DescBoundTextBox
            // 
            this.BindingSource.SetBindingMember(this.GG_DescBoundTextBox, "GG_Desc");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).GG_Desc)));
            this.GG_DescBoundTextBox.CaptionResourceString = null;
            this.GG_DescBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
            this.GG_DescBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 35, true);
            this.GG_DescBoundTextBox.Name = "GG_DescBoundTextBox";
            this.GG_DescBoundTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.GG_DescBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(208, 20, true);
            this.GG_DescBoundTextBox.TabIndex = 11;
            // 
            // GG_CodeBoundTextBox
            // 
            this.BindingSource.SetBindingMember(this.GG_CodeBoundTextBox, "GG_Code");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).GG_Code)));
            this.GG_CodeBoundTextBox.CaptionResourceString = null;
            this.GG_CodeBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 11, true);
            this.GG_CodeBoundTextBox.Name = "GG_CodeBoundTextBox";
            this.GG_CodeBoundTextBox.ShouldEscapeAllSpecialCharacters = false;
            this.GG_CodeBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
            this.GG_CodeBoundTextBox.TabIndex = 10;
            // 
            // ParentGroupFindBox
            // 
            this.ParentGroupFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.ParentGroupFindBox, "GG_GG_ParentGroup");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbGroup)(null)).GG_GG_ParentGroup)));
            this.ParentGroupFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(397, 61, true);
            this.ParentGroupFindBox.Name = "ParentGroupFindBox";
            this.ParentGroupFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.ParentGroupFindBox.ParentType = null;
            this.ParentGroupFindBox.PreBoundMaxLength = 15;
            this.ParentGroupFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(231, 20, true);
            this.ParentGroupFindBox.TabIndex = 18;
            // 
            // GlbGroupForm
            // 
            this.CaptionRenderingEnabled = true;
            this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbGroupForm|f1891378-a51e-47b3-9e65-f3d4ea76c15f", "Group");
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(884, 574, true);
            this.Controls.Add(this.GroupTabControl);
            this.Controls.Add(this.BottomPanel);
            this.Controls.Add(this.TopPanel);
            this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbGroup);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(950, 538, true);
            this.Name = "GlbGroupForm";
            this.ShouldSerializeTabPageMethods = false;
            this.Load += new System.EventHandler(this.GlbGroupForm_Load);
            this.Controls.SetChildIndex(this.MainStatusBar, 0);
            this.Controls.SetChildIndex(this.TopPanel, 0);
            this.Controls.SetChildIndex(this.BottomPanel, 0);
            this.Controls.SetChildIndex(this.GroupTabControl, 0);
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.GroupTabControl.ResumeLayout(false);
            this.GroupTabControl.PerformLayout();
            this.MembersTabPage.ResumeLayout(false);
            this.MembersTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MembersModuleButtonGrid.InnerGrid)).EndInit();
            this.MembersModuleButtonGrid.ResumeLayout(true);
            this.MembersModuleButtonGrid.PerformLayout();
            this.SecurityTabPage.ResumeLayout(false);
            this.SecurityTabPage.PerformLayout();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.splitContainer1.PerformLayout();
            this.SecurityTreePanel.ResumeLayout(false);
            this.SecurityTreePanel.PerformLayout();
            this.MainSecurityRightsPanel.ResumeLayout(false);
            this.MainSecurityRightsPanel.PerformLayout();
            this.ActualSecurityPanel.ResumeLayout(false);
            this.ActualSecurityPanel.PerformLayout();
            this.SecurityPanel.ResumeLayout(false);
            this.SecurityPanel.PerformLayout();
            this.EffectiveSecurityRightsGroupBox.ResumeLayout(false);
            this.EffectiveSecurityRightsGroupBox.PerformLayout();
            this.SecurityDepartmentGuidFindBox.ResumeLayout(true);
            this.SecurityDepartmentGuidFindBox.PerformLayout();
            this.SecurityBranchGuidFindBox.ResumeLayout(true);
            this.SecurityBranchGuidFindBox.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.SecurityGrid)).EndInit();
            this.SecurityGrid.ResumeLayout(false);
            this.SecurityGrid.PerformLayout();
            this.AllowedOrgsAndWarehousesSecurityPanel.ResumeLayout(true);
            this.AllowedOrgsAndWarehousesSecurityPanel.PerformLayout();
            this.changeOthersSecurityPanel.ResumeLayout(false);
            this.changeOthersSecurityPanel.PerformLayout();
            this.changeOthersSecurityControl.ResumeLayout(true);
            this.changeOthersSecurityControl.PerformLayout();
            this.CustomFieldsTabPage.ResumeLayout(false);
            this.CustomFieldsTabPage.PerformLayout();
            this.customFieldsControl.ResumeLayout(true);
            this.customFieldsControl.PerformLayout();
            this.WorkflowTabPage.ResumeLayout(false);
            this.WorkflowTabPage.PerformLayout();
            this.GroupOwnersTabPage.ResumeLayout(false);
            this.GroupOwnersTabPage.PerformLayout();
            this.GroupOwnersSecurityPanel.ResumeLayout(false);
            this.GroupOwnersSecurityPanel.PerformLayout();
            this.GroupOwnersSecurityControl.ResumeLayout(true);
            this.GroupOwnersSecurityControl.PerformLayout();
            this.OrganisationsTabPage.ResumeLayout(false);
            this.OrganisationsTabPage.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.OrganisationsModuleButtonGrid.InnerGrid)).EndInit();
            this.OrganisationsModuleButtonGrid.ResumeLayout(true);
            this.OrganisationsModuleButtonGrid.PerformLayout();
            this.zStmNoteTabPage1.ResumeLayout(false);
            this.zStmNoteTabPage1.PerformLayout();
            this.DomainNameDropEdit.ResumeLayout(true);
            this.DomainNameDropEdit.PerformLayout();
            this.BottomPanel.ResumeLayout(false);
            this.BottomPanel.PerformLayout();
            this.PostingButtonsUserControl.ResumeLayout(true);
            this.PostingButtonsUserControl.PerformLayout();
            this.TopPanel.ResumeLayout(false);
            this.TopPanel.PerformLayout();
            this.CategoryDropEdit.ResumeLayout(true);
            this.CategoryDropEdit.PerformLayout();
            this.ParentGroupFindBox.ResumeLayout(true);
            this.ParentGroupFindBox.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}
		#endregion

		private Enterprise.MasterFiles.GUI.AllowedOrgsAndWarehousesControl AllowedOrgsAndWarehousesSecurityPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		private Enterprise.Core.Forms.ZPostingButtonsUserControl PostingButtonsUserControl;
		private Enterprise.ZArchitecture.GUI.ZPanel TopPanel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox GG_IsActiveCheckBox;
		private Enterprise.ZArchitecture.ZTextBox GG_DescBoundTextBox;
		private Enterprise.ZArchitecture.ZTextBox GG_CodeBoundTextBox;
		internal Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		internal Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		internal Enterprise.ZArchitecture.GUI.ZTabPage MembersTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage SecurityTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage CustomFieldsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage GroupOwnersTabPage;
		internal Enterprise.ZArchitecture.GUI.ZTabPage OrganisationsTabPage;
		internal Enterprise.ZArchitecture.GUI.ZModuleButtonGridWithBlankDetachMessage MembersModuleButtonGrid;
		internal Enterprise.ZArchitecture.GUI.ZModuleButtonGrid OrganisationsModuleButtonGrid;
		internal Enterprise.ZArchitecture.ZLabel GroupSecurityRightLabel;
		internal Enterprise.ZArchitecture.ZLabel SecurityFunctionPathLabel;
		private Enterprise.ZArchitecture.ZLabel GroupSecurityForLabel;
		private Enterprise.ZArchitecture.ZLabel SecurityFunctionLabel;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox SecurityBranchGuidFindBox;
		internal Enterprise.ZArchitecture.GUI.ZGuidFindBox SecurityDepartmentGuidFindBox;
		private Enterprise.ZArchitecture.ZLabel SecurityGrantedLabel;
		internal Enterprise.ZArchitecture.GUI.ZTemplateTabControl GroupTabControl;
		private Enterprise.ZArchitecture.GUI.ZPanel SecurityPanel;
		private Enterprise.ZArchitecture.ZGrid SecurityGrid;
		internal Enterprise.ZArchitecture.ZLabel tabSecurityLabel;
		internal Enterprise.ZArchitecture.ZLabel groupHintLabel;
		private Enterprise.ZArchitecture.GUI.ZPanel ActualSecurityPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel SecurityTreePanel;
		private Enterprise.ZArchitecture.GUI.ZPanel MainSecurityRightsPanel;
		private CargoWise.Windows.UI.KSplitter OrgsAndWarehousesSplitter;
		private ChangeOthersSecurityControl changeOthersSecurityControl;
		internal ChangeOthersSecurityControl GroupOwnersSecurityControl;
		internal Enterprise.ZArchitecture.GUI.ZGroupBox EffectiveSecurityRightsGroupBox;
		private Enterprise.ZArchitecture.GUI.ZPanel changeOthersSecurityPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel GroupOwnersSecurityPanel;
		private Enterprise.ZArchitecture.GUI.ZPanel DynamicSecurityPanel;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox ADLinkedCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZCheckBox NonSecurityCheckBox;
		internal ZArchitecture.GUI.ZDropEditWithFixedWidth DomainNameDropEdit;
		private Enterprise.ZArchitecture.ZLabel MembersHintLabel;
		private Enterprise.ZArchitecture.GUI.ProcessTemplateCustomFieldsControl customFieldsControl;
		internal Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		private ZArchitecture.GUI.ZDropEdit CategoryDropEdit;
		private Enterprise.ZArchitecture.GUI.ZGuidFindBox ParentGroupFindBox;
	}
}
