using System;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MasterFiles.GUI
{
	public partial class GlbResourceForm : ZForm
	{
		Enterprise.ZArchitecture.ZTextBox GS_FullNameBoundTextBox;
		Enterprise.ZArchitecture.GUI.ZTemplateTabControl GlbStaffTabControl;
		Enterprise.ZArchitecture.GUI.ZDateEdit DepartDateEdit;
		Enterprise.ZArchitecture.GUI.ZPanel MainPanel;
		Enterprise.ZArchitecture.GUI.ZPanel BottomPanel;
		Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		Enterprise.ZArchitecture.GUI.ZTabPage DetailsTabPage;
		Enterprise.ZArchitecture.GUI.ZGroupBox EmployeeDetailsGroupBox;
		Enterprise.ZArchitecture.GUI.ZDateEdit BirthDateDateEdit;
		Enterprise.ZArchitecture.ZTextBox GS_UserAddress1BoundText;
		Enterprise.ZArchitecture.GUI.ZStmNoteTabPage zStmNoteTabPage1;
		Enterprise.ZArchitecture.GUI.ZLogsTabPage zLogsTabPage1;
		Enterprise.ZArchitecture.GUI.ZGroupBox HomeBranchDepartmentGroupBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox HomeDepartmentFindBox;
		Enterprise.ZArchitecture.GUI.ZGuidFindBox HomeBranchGuidFindBox;
		private ZCheckBox GS_IsActiveBoundCheck;
		private ZGrid GlbCertificatesBoundGrid;
		private ZTabPage AllocationTabPage;
		private ZTabPage AvailTabPage;
		private GlbWorkTimeControl glbWorkTimeControl;
		private ZDropEdit GS_ResourceTypeDropEdit;
		private ZTextBox GS_CodeTextBox;
		private ZTextBox GS_EmailAddressBoundText;
		private CargoWise.Windows.UI.KPanel allocationPanel;
		private ZGroupBox FilterGroupBox;
		private ZDateEdit AllocationEndTimeDateEdit;
		private ZDateEdit AllocationStartTimeDateEdit;
		private ZGrid TimeAllocationGrid;
		private ZTabPage CapabilitiesTabPage;
		private ResourceCapabilityUserControl resourceCapabilityUserControl1;
		private ZTabPage ComponentMembershipTabPage;
		private ResourceBMComponentMembershipControl ResourceBMComponentMembershipControl;

		new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			this.GlbStaffTabControl = new Enterprise.ZArchitecture.GUI.ZTemplateTabControl();
			this.DetailsTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AllocationTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.AvailTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.CapabilitiesTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.zStmNoteTabPage1 = new Enterprise.ZArchitecture.GUI.ZStmNoteTabPage();
			this.zLogsTabPage1 = new Enterprise.ZArchitecture.GUI.ZLogsTabPage();
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.MainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ComponentMembershipTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.GlbStaffTabControl.SuspendLayout();
			this.BottomPanel.SuspendLayout();
			this.MainPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 368, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 22, true);
			this.MainStatusBar.TabIndex = 1;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(817);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaff);
			// 
			// GlbStaffTabControl
			// 
			this.GlbStaffTabControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)));
			this.GlbStaffTabControl.Controls.Add(this.DetailsTabPage);
			this.GlbStaffTabControl.Controls.Add(this.AllocationTabPage);
			this.GlbStaffTabControl.Controls.Add(this.AvailTabPage);
			this.GlbStaffTabControl.Controls.Add(this.CapabilitiesTabPage);
			this.GlbStaffTabControl.Controls.Add(this.ComponentMembershipTabPage);
			this.GlbStaffTabControl.Controls.Add(this.zStmNoteTabPage1);
			this.GlbStaffTabControl.Controls.Add(this.zLogsTabPage1);
			this.GlbStaffTabControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.GlbStaffTabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.GlbStaffTabControl.Name = "GlbStaffTabControl";
			this.GlbStaffTabControl.SelectedIndex = 0;
			this.GlbStaffTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 332, true);
			this.GlbStaffTabControl.TabIndex = 0;
			this.GlbStaffTabControl.TabStop = false;
			// 
			// DetailsTabPage
			// 
			this.DetailsTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|f1e6901d-97cc-4b1b-ae52-6d2a9312889e", "Details");
			this.DetailsTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.DetailsTabPage.Name = "DetailsTabPage";
			this.DetailsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.DetailsTabPage.TabIndex = 2;
			this.DetailsTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.DetailsTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_Code)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_ResourceType)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_IsActive)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_FullName)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_DepartureDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_Birthdate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_UserAddress1)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_GE_HomeDepartment)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_GB_HomeBranch)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Certificates)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Certificates)).SyncRoot)).XZ_Type)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Certificates)).SyncRoot)).XZ_Comment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Certificates)).SyncRoot)).XZ_RefNumber)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GenRegCertAccredMaintList)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).Certificates)).SyncRoot)).XZ_ExpiryOrDueDate)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).GS_EmailAddress)));
			// 
			// AllocationTabPage
			// 
			this.AllocationTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|12de4f57-737d-4fea-aed1-74f27ccb425a", "Allocation");
			this.AllocationTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AllocationTabPage.Name = "AllocationTabPage";
			this.AllocationTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AllocationTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.AllocationTabPage.TabIndex = 12;
			this.AllocationTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AllocationTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.EndTime)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.StartTime)));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbTimeAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)).SyncRoot)).GA_WorkHolidayType)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbTimeAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)).SyncRoot)).TypeDescription)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbTimeAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)).SyncRoot)).GA_StartTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.MasterFiles.Business.GlbTimeAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)).SyncRoot)).GA_EndTime)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbTimeAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)).SyncRoot)).GA_LeaveComment)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.GlbTimeAllocation)(((System.Collections.IList)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).TimeAllocationFilter.AllocationsView)).SyncRoot)).RelatedItemTypeDescription)));
			// 
			// AvailTabPage
			// 
			this.AvailTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|86bb0453-e152-4833-a174-b5dac979c1a7", "Availability");
			this.AvailTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.AvailTabPage.Name = "AvailTabPage";
			this.AvailTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.AvailTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.AvailTabPage.TabIndex = 13;
			this.AvailTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.AvailTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.GlbWorkTimeViewModel)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).WorkTimeViewModel)));
			// 
			// CapabilitiesTabPage
			// 
			this.CapabilitiesTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("59e5b150-4424-44db-9076-66a36703f790", "Capabilities");
			this.CapabilitiesTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.CapabilitiesTabPage.Name = "CapabilitiesTabPage";
			this.CapabilitiesTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.CapabilitiesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.CapabilitiesTabPage.TabIndex = 14;
			this.CapabilitiesTabPage.UseVisualStyleBackColor = true;
			this.CapabilitiesTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.CapabilitiesTabPage_InitializeTab));
			// 
			// zStmNoteTabPage1
			// 
			this.zStmNoteTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zStmNoteTabPage1.Name = "zStmNoteTabPage1";
			this.zStmNoteTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.zStmNoteTabPage1.TabIndex = 9;
			// 
			// zLogsTabPage1
			// 
			this.zLogsTabPage1.ExcludeFromBindingOnSave = true;
			this.zLogsTabPage1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.zLogsTabPage1.Name = "zLogsTabPage1";
			this.zLogsTabPage1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.zLogsTabPage1.TabIndex = 10;
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ButtonsUserControl);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 332, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 36, true);
			this.BottomPanel.TabIndex = 0;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.AllowDrop = true;
			this.ButtonsUserControl.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(590, 4, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 24, true);
			this.ButtonsUserControl.TabIndex = 0;
			// 
			// MainPanel
			// 
			this.MainPanel.Controls.Add(this.GlbStaffTabControl);
			this.MainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.MainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.MainPanel.Name = "MainPanel";
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 332, true);
			this.MainPanel.TabIndex = 12;
			// 
			// ComponentMembershipTabPage
			// 
			this.ComponentMembershipTabPage.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("da508ccf-ba3d-4bc4-84f5-d6aaea83ba50", "Component Membership");
			this.ComponentMembershipTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.ComponentMembershipTabPage.Name = "ComponentMembershipTabPage";
			this.ComponentMembershipTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.ComponentMembershipTabPage.TabIndex = 15;
			this.ComponentMembershipTabPage.RunWhenBindingOrFirstShown(new System.EventHandler(this.ComponentMembershipTabPage_InitializeTab));
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.BufferManagement.Integration.IBMComponentResourceLinkCollection)(((Enterprise.MasterFiles.Business.GlbStaff)(null)).ComponentMembership)));
			// 
			// GlbResourceForm
			// 
			this.AutoAddPreviousNextButtons = false;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("d01b4a19-29c0-44aa-9759-cc897a4670e3", "Resource");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(832, 390, true);
			this.Controls.Add(this.MainPanel);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbStaff);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(840, 426, true);
			this.Name = "GlbResourceForm";
			this.ShouldSerializeTabPageMethods = true;
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.MainPanel, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.GlbStaffTabControl.ResumeLayout(false);
			this.BottomPanel.ResumeLayout(false);
			this.MainPanel.ResumeLayout(false);
			this.ResumeLayout(false);
		}

		private void DetailsTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo3 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.EmployeeDetailsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.GS_CodeTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.GS_ResourceTypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.GS_IsActiveBoundCheck = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.GS_FullNameBoundTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.DepartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.BirthDateDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.GS_UserAddress1BoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.HomeBranchDepartmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.HomeDepartmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.HomeBranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.GlbCertificatesBoundGrid = new Enterprise.ZArchitecture.ZGrid();
			this.GS_EmailAddressBoundText = new Enterprise.ZArchitecture.ZTextBox();
			this.DetailsTabPage.SuspendLayout();
			this.EmployeeDetailsGroupBox.SuspendLayout();
			this.HomeBranchDepartmentGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.GlbCertificatesBoundGrid)).BeginInit();
			this.DetailsTabPage.Controls.Add(this.EmployeeDetailsGroupBox);
			this.DetailsTabPage.Controls.Add(this.HomeBranchDepartmentGroupBox);
			this.DetailsTabPage.Controls.Add(this.GlbCertificatesBoundGrid);
			// 
			// EmployeeDetailsGroupBox
			// 
			this.EmployeeDetailsGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|9a04e487-4f47-4b57-8680-86d532a7d460", "Resource Details");
			this.EmployeeDetailsGroupBox.Controls.Add(this.GS_EmailAddressBoundText);
			this.EmployeeDetailsGroupBox.Controls.Add(this.GS_CodeTextBox);
			this.EmployeeDetailsGroupBox.Controls.Add(this.GS_ResourceTypeDropEdit);
			this.EmployeeDetailsGroupBox.Controls.Add(this.GS_IsActiveBoundCheck);
			this.EmployeeDetailsGroupBox.Controls.Add(this.GS_FullNameBoundTextBox);
			this.EmployeeDetailsGroupBox.Controls.Add(this.DepartDateEdit);
			this.EmployeeDetailsGroupBox.Controls.Add(this.BirthDateDateEdit);
			this.EmployeeDetailsGroupBox.Controls.Add(this.GS_UserAddress1BoundText);
			this.EmployeeDetailsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.EmployeeDetailsGroupBox.Name = "EmployeeDetailsGroupBox";
			this.EmployeeDetailsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 202, true);
			this.EmployeeDetailsGroupBox.TabIndex = 0;
			this.EmployeeDetailsGroupBox.TabStop = false;
			// 
			// GS_CodeTextBox
			// 
			this.BindingSource.SetBindingMember(this.GS_CodeTextBox, "GS_Code");
			this.GS_CodeTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|447d2eec-2ae1-4388-b171-a1bdf202c9a1", "Code");
			this.GS_CodeTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 20, true);
			this.GS_CodeTextBox.Name = "GS_CodeTextBox";
			this.GS_CodeTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(51, 20, true);
			this.GS_CodeTextBox.TabIndex = 1;
			// 
			// GS_ResourceTypeDropEdit
			// 
			this.GS_ResourceTypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GS_ResourceTypeDropEdit, "GS_ResourceType");
			this.GS_ResourceTypeDropEdit.DisableInvalidation = false;
			this.GS_ResourceTypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 102, true);
			this.GS_ResourceTypeDropEdit.Name = "GS_ResourceTypeDropEdit";
			this.GS_ResourceTypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.GS_ResourceTypeDropEdit.TabIndex = 8;
			// 
			// GS_IsActiveBoundCheck
			// 
			this.GS_IsActiveBoundCheck.AutoSize = true;
			this.BindingSource.SetBindingMember(this.GS_IsActiveBoundCheck, "GS_IsActive");
			this.GS_IsActiveBoundCheck.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.GS_IsActiveBoundCheck.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GS_IsActiveBoundCheck.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(319, 23, true);
			this.GS_IsActiveBoundCheck.Name = "GS_IsActiveBoundCheck";
			this.GS_IsActiveBoundCheck.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.GS_IsActiveBoundCheck.TabIndex = 2;
			this.GS_IsActiveBoundCheck.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			// 
			// GS_FullNameBoundTextBox
			// 
			this.BindingSource.SetBindingMember(this.GS_FullNameBoundTextBox, "GS_FullName");
			this.GS_FullNameBoundTextBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|521fd125-0730-474c-abe8-0bca2a391442", "Name");
			this.GS_FullNameBoundTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GS_FullNameBoundTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 48, true);
			this.GS_FullNameBoundTextBox.Name = "GS_FullNameBoundTextBox";
			this.GS_FullNameBoundTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.GS_FullNameBoundTextBox.TabIndex = 4;
			// 
			// DepartDateEdit
			// 
			this.DepartDateEdit.AllowDrop = true;
			this.DepartDateEdit.AutoCompleteMonthThreshold = 1;
			this.DepartDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DepartDateEdit, "GS_DepartureDate");
			this.DepartDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|614fe094-5010-4baf-b179-369d9d49d2eb", "Disposed");
			this.DepartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(251, 163, true);
			this.DepartDateEdit.Name = "DepartDateEdit";
			this.DepartDateEdit.TabIndex = 14;
			// 
			// BirthDateDateEdit
			// 
			this.BirthDateDateEdit.AllowDrop = true;
			this.BirthDateDateEdit.AutoCompleteMonthThreshold = 1;
			this.BirthDateDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.BirthDateDateEdit, "GS_Birthdate");
			this.BirthDateDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|6dad958d-1dec-4b7b-821b-f8dafc04179b", "Purchased");
			this.BirthDateDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(89, 163, true);
			this.BirthDateDateEdit.Name = "BirthDateDateEdit";
			this.BirthDateDateEdit.TabIndex = 12;
			// 
			// GS_UserAddress1BoundText
			// 
			this.BindingSource.SetBindingMember(this.GS_UserAddress1BoundText, "GS_UserAddress1");
			this.GS_UserAddress1BoundText.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|290d9f07-3324-450b-85cc-4970c2c9be3a", "Location");
			this.GS_UserAddress1BoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GS_UserAddress1BoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 133, true);
			this.GS_UserAddress1BoundText.Name = "GS_UserAddress1BoundText";
			this.GS_UserAddress1BoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.GS_UserAddress1BoundText.TabIndex = 10;
			// 
			// HomeBranchDepartmentGroupBox
			// 
			this.HomeBranchDepartmentGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|96c07899-a566-4d91-a953-5ce00eec6127", "Home Branch / Department");
			this.HomeBranchDepartmentGroupBox.Controls.Add(this.HomeDepartmentFindBox);
			this.HomeBranchDepartmentGroupBox.Controls.Add(this.HomeBranchGuidFindBox);
			this.HomeBranchDepartmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 216, true);
			this.HomeBranchDepartmentGroupBox.Name = "HomeBranchDepartmentGroupBox";
			this.HomeBranchDepartmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(354, 88, true);
			this.HomeBranchDepartmentGroupBox.TabIndex = 1;
			this.HomeBranchDepartmentGroupBox.TabStop = false;
			// 
			// HomeDepartmentFindBox
			// 
			this.HomeDepartmentFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HomeDepartmentFindBox, "GS_GE_HomeDepartment");
			this.HomeDepartmentFindBox.DisableInvalidation = false;
			this.HomeDepartmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 52, true);
			this.HomeDepartmentFindBox.Name = "HomeDepartmentFindBox";
			this.HomeDepartmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.HomeDepartmentFindBox.TabIndex = 3;
			// 
			// HomeBranchGuidFindBox
			// 
			this.HomeBranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.HomeBranchGuidFindBox, "GS_GB_HomeBranch");
			this.HomeBranchGuidFindBox.DisableInvalidation = false;
			this.HomeBranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 28, true);
			this.HomeBranchGuidFindBox.Name = "HomeBranchGuidFindBox";
			this.HomeBranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.HomeBranchGuidFindBox.TabIndex = 1;
			// 
			// GlbCertificatesBoundGrid
			// 
			this.GlbCertificatesBoundGrid.AllowNavigation = false;
			this.BindingSource.SetBindingMember(this.GlbCertificatesBoundGrid, "Certificates");
			this.GlbCertificatesBoundGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo3.ColumnName = "XZ_Type";
			zTextBoxColumnStyleInfo6.ColumnName = "XZ_Comment";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo7.ColumnName = "XZ_RefNumber";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zDateEditColumnStyleInfo4.ColumnName = "XZ_ExpiryOrDueDate";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			this.GlbCertificatesBoundGrid.ColumnStyles.Add(zDropEditColumnStyleInfo3);
			this.GlbCertificatesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.GlbCertificatesBoundGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.GlbCertificatesBoundGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.GlbCertificatesBoundGrid.CopySelectedRowsAllowed = true;
			this.GlbCertificatesBoundGrid.GridId = "6ae1147b-aa0b-406e-a70e-0f08f4cf0b89";
			this.GlbCertificatesBoundGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.GlbCertificatesBoundGrid.LayoutKey = "oGrid1";
			this.GlbCertificatesBoundGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(378, 8, true);
			this.GlbCertificatesBoundGrid.Name = "GlbCertificatesBoundGrid";
			this.GlbCertificatesBoundGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(438, 296, true);
			this.GlbCertificatesBoundGrid.TabIndex = 2;
			// 
			// GS_EmailAddressBoundText
			// 
			this.GS_EmailAddressBoundText.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.GS_EmailAddressBoundText, "GS_EmailAddress");
			this.GS_EmailAddressBoundText.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.GS_EmailAddressBoundText.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(88, 74, true);
			this.GS_EmailAddressBoundText.Name = "GS_EmailAddressBoundText";
			this.GS_EmailAddressBoundText.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(246, 20, true);
			this.GS_EmailAddressBoundText.TabIndex = 6;
			this.EmployeeDetailsGroupBox.ResumeLayout(false);
			this.EmployeeDetailsGroupBox.PerformLayout();
			this.HomeBranchDepartmentGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.GlbCertificatesBoundGrid)).EndInit();
			this.DetailsTabPage.ResumeLayout(true);
		}

		private void AllocationTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo5 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			this.allocationPanel = new CargoWise.Windows.UI.KPanel();
			this.FilterGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.AllocationEndTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.AllocationStartTimeDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.TimeAllocationGrid = new Enterprise.ZArchitecture.ZGrid();
			this.AllocationTabPage.SuspendLayout();
			this.allocationPanel.SuspendLayout();
			this.FilterGroupBox.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.TimeAllocationGrid)).BeginInit();
			this.AllocationTabPage.Controls.Add(this.allocationPanel);
			// 
			// allocationPanel
			// 
			this.allocationPanel.Controls.Add(this.FilterGroupBox);
			this.allocationPanel.Controls.Add(this.TimeAllocationGrid);
			this.allocationPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.allocationPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.allocationPanel.Name = "allocationPanel";
			this.allocationPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 299, true);
			this.allocationPanel.TabIndex = 0;
			// 
			// FilterGroupBox
			// 
			this.FilterGroupBox.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|ea0ea6bd-9d7a-4f6e-868c-8ca144dff70a", "Filter");
			this.FilterGroupBox.Controls.Add(this.AllocationEndTimeDateEdit);
			this.FilterGroupBox.Controls.Add(this.AllocationStartTimeDateEdit);
			this.FilterGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 3, true);
			this.FilterGroupBox.Name = "FilterGroupBox";
			this.FilterGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(381, 39, true);
			this.FilterGroupBox.TabIndex = 216;
			this.FilterGroupBox.TabStop = false;
			// 
			// AllocationEndTimeDateEdit
			// 
			this.AllocationEndTimeDateEdit.AllowDrop = true;
			this.AllocationEndTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.AllocationEndTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AllocationEndTimeDateEdit, "TimeAllocationFilter+EndTime");
			this.AllocationEndTimeDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|365a2f3e-7c09-471e-ad96-70345112706b", "End Time");
			this.AllocationEndTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(276, 14, true);
			this.AllocationEndTimeDateEdit.Name = "AllocationEndTimeDateEdit";
			this.AllocationEndTimeDateEdit.TabIndex = 217;
			// 
			// AllocationStartTimeDateEdit
			// 
			this.AllocationStartTimeDateEdit.AllowDrop = true;
			this.AllocationStartTimeDateEdit.AutoCompleteMonthThreshold = 1;
			this.AllocationStartTimeDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.AllocationStartTimeDateEdit, "TimeAllocationFilter+StartTime");
			this.AllocationStartTimeDateEdit.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|1cebca53-29f4-48ff-b1e5-cec00ecb2d69", "Start Time");
			this.AllocationStartTimeDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(76, 14, true);
			this.AllocationStartTimeDateEdit.Name = "AllocationStartTimeDateEdit";
			this.AllocationStartTimeDateEdit.TabIndex = 216;
			// 
			// TimeAllocationGrid
			// 
			this.TimeAllocationGrid.AllowNavigation = false;
			this.TimeAllocationGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
						| System.Windows.Forms.AnchorStyles.Left)
						| System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.TimeAllocationGrid, "TimeAllocationFilter+AllocationsView");
			this.TimeAllocationGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "GA_WorkHolidayType";
			zDropEditColumnStyleInfo1.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|343154ed-c1e4-47da-8346-56bf302bf442", "Type");
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|ebb67a72-ef36-4cbb-a87c-423e454fd30f", "Type Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TypeDescription";
			zTextBoxColumnStyleInfo1.GroupName = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|343154ed-c1e4-47da-8346-56bf302bf442", "Type");
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zDateEditColumnStyleInfo1.ColumnName = "GA_StartTime";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zDateEditColumnStyleInfo5.ColumnName = "GA_EndTime";
			zDateEditColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			zTextBoxColumnStyleInfo2.ColumnName = "GA_LeaveComment";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("GlbResourceForm|28bb20c8-5a39-453d-b136-ade8b32abd2e", "Related Job");
			zTextBoxColumnStyleInfo8.ColumnName = "RelatedItemTypeDescription";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			this.TimeAllocationGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.TimeAllocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.TimeAllocationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.TimeAllocationGrid.ColumnStyles.Add(zDateEditColumnStyleInfo5);
			this.TimeAllocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.TimeAllocationGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.TimeAllocationGrid.CopySelectedRowsAllowed = true;
			this.TimeAllocationGrid.GridId = "25c89921-f039-4861-bebb-66d4df8f5520";
			this.TimeAllocationGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.TimeAllocationGrid.LayoutKey = "zGrid1";
			this.TimeAllocationGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(5, 43, true);
			this.TimeAllocationGrid.Name = "TimeAllocationGrid";
			this.TimeAllocationGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 253, true);
			this.TimeAllocationGrid.TabIndex = 211;
			this.allocationPanel.ResumeLayout(false);
			this.FilterGroupBox.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.TimeAllocationGrid)).EndInit();
			this.AllocationTabPage.ResumeLayout(true);
		}

		private void AvailTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.glbWorkTimeControl = new Enterprise.MasterFiles.GUI.GlbWorkTimeControl();
			this.AvailTabPage.SuspendLayout();
			this.AvailTabPage.Controls.Add(this.glbWorkTimeControl);
			// 
			// glbWorkTimeControl
			// 
			this.glbWorkTimeControl.AllowDrop = true;
			this.glbWorkTimeControl.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
			this.BindingSource.SetBindingMember(this.glbWorkTimeControl, "WorkTimeViewModel");
			this.glbWorkTimeControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 12, true);
			this.glbWorkTimeControl.Name = "glbWorkTimeControl";
			this.glbWorkTimeControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(808, 235, true);
			this.glbWorkTimeControl.TabIndex = 214;
			this.AvailTabPage.ResumeLayout(true);
		}

		private void CapabilitiesTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.resourceCapabilityUserControl1 = new Enterprise.MasterFiles.GUI.ResourceCapabilityUserControl();
			this.CapabilitiesTabPage.SuspendLayout();
			this.CapabilitiesTabPage.Controls.Add(this.resourceCapabilityUserControl1);
			// 
			// resourceCapabilityUserControl1
			// 
			this.resourceCapabilityUserControl1.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.resourceCapabilityUserControl1, ".");
			this.resourceCapabilityUserControl1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.resourceCapabilityUserControl1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.resourceCapabilityUserControl1.Name = "resourceCapabilityUserControl1";
			this.resourceCapabilityUserControl1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(818, 299, true);
			this.resourceCapabilityUserControl1.TabIndex = 0;
			this.CapabilitiesTabPage.ResumeLayout(true);
		}

		private void ComponentMembershipTabPage_InitializeTab(object sender, EventArgs e)
		{
			// 
			// ControlCodeDomSerializerWithDelayedTabCreate Designer generated code
			// 
			this.ResourceBMComponentMembershipControl = new Enterprise.MasterFiles.GUI.ResourceBMComponentMembershipControl();
			this.ComponentMembershipTabPage.SuspendLayout();
			this.ComponentMembershipTabPage.Controls.Add(this.ResourceBMComponentMembershipControl);
			// 
			// ResourceBMComponentMembershipControl
			// 
			this.ResourceBMComponentMembershipControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ResourceBMComponentMembershipControl, "ComponentMembership");
			this.ResourceBMComponentMembershipControl.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ResourceBMComponentMembershipControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ResourceBMComponentMembershipControl.Name = "ResourceBMComponentMembershipControl";
			this.ResourceBMComponentMembershipControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(824, 305, true);
			this.ResourceBMComponentMembershipControl.TabIndex = 0;
			this.ResourceBMComponentMembershipControl.CaptionRenderingEnabled = true;
			this.ComponentMembershipTabPage.ResumeLayout(true);
		}

		#region Dispose

		System.ComponentModel.IContainer components = null;
		protected override void Dispose(bool isNotFinalizing)
		{
			if (isNotFinalizing)
			{
				if (components != null)
				{
					components.Dispose();
				}
			}
			base.Dispose(isNotFinalizing);
		}

		#endregion
	}
}
