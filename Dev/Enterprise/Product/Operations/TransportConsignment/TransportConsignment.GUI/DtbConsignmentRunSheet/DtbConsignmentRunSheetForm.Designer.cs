using CargoWise.Types;
using Enterprise.ZArchitecture.GUI;
namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbConsignmentRunSheetForm
	{
		/// <summary>
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		new void InitializeComponent()
		{
            this.RunSheetMainPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.RunSheetBottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.InstructionsSplitPanel = new CargoWise.Windows.UI.KSplitContainer();
            this.RunSheetInstructionsControl = new Enterprise.TransportConsignment.GUI.DtbConsignmentRunSheetInstructionsUserControl();
            this.ControlBoxToolStrip = new Enterprise.ZArchitecture.GUI.ZToolStrip();
            this.DetailsButton = new Enterprise.ZArchitecture.GUI.ZToolStripButton();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.ConsignmentsUserControl = new Enterprise.TransportConsignment.GUI.DtbConsignmentRunSheetDetailsUserControl();
            this.RunSheetTopPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
            this.TopGroupBox = new Enterprise.TransportConsignment.GUI.ZGroupBoxWithoutCaption();
            this.GlowLinkLabel = new Enterprise.ZArchitecture.GUI.ZLinkLabel();
            this.GroupBoxSeparator = new Enterprise.TransportConsignment.GUI.ZGroupBoxWithoutCaption();
            this.AdHocRegoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdHocLicenseTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdHocTransportCoTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.RunSheetStatusTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.AdHocDriverTextBox = new Enterprise.ZArchitecture.ZTextBox();
            this.TransportCoFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.StaffDriverFindBox = new Enterprise.ZArchitecture.GUI.ZCodeFindBox();
            this.VechicleFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
            this.EndDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.StartDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
            this.DurationTimeEditEx = new Enterprise.ZArchitecture.GUI.ZTimeEditEx();
            this.IsHazardousCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.RequiresRefrigerationCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
            this.WorkflowTabPage = new Enterprise.MasterFiles.GUI.ZWorkflowTabPage();
            this.MainTabControl.SuspendLayout();
            this.MainTabPage.SuspendLayout();
            this.NotesTabPage.SuspendLayout();
            this.MainPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
            this.RunSheetMainPanel.SuspendLayout();
            this.RunSheetBottomPanel.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.InstructionsSplitPanel)).BeginInit();
            this.InstructionsSplitPanel.Panel1.SuspendLayout();
            this.InstructionsSplitPanel.Panel2.SuspendLayout();
            this.InstructionsSplitPanel.SuspendLayout();
            this.RunSheetInstructionsControl.SuspendLayout();
            this.ControlBoxToolStrip.SuspendLayout();
            this.ConsignmentsUserControl.SuspendLayout();
            this.RunSheetTopPanel.SuspendLayout();
            this.TopGroupBox.SuspendLayout();
            this.TransportCoFindBox.SuspendLayout();
            this.StaffDriverFindBox.SuspendLayout();
            this.VechicleFindBox.SuspendLayout();
            this.EndDateEdit.SuspendLayout();
            this.StartDateEdit.SuspendLayout();
            this.WorkflowTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // MainTabControl
            // 
            this.MainTabControl.Controls.Add(this.WorkflowTabPage);
            this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 630, true);
            this.MainTabControl.TabIndex = 0;
            this.MainTabControl.Controls.SetChildIndex(this.NotesTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.LogsTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.WorkflowTabPage, 0);
            this.MainTabControl.Controls.SetChildIndex(this.MainTabPage, 0);
            // 
            // MainTabPage
            // 
            this.MainTabPage.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("ZTemplateForm|ab70d691-1e63-4fad-af03-992c9654c374", "Run Sheet");
            this.MainTabPage.Controls.Add(this.RunSheetMainPanel);
            this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 603, true);
            // 
            // NotesTabPage
            // 
            this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 603, true);
            // 
            // LogsTabPage
            // 
            this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 603, true);
            // 
            // MainPanel
            // 
            this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 630, true);
            // 
            // MainStatusBar
            // 
            this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 24, true);
            // 
            // BindingSource
            // 
            this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet);
            // 
            // RunSheetMainPanel
            // 
            this.RunSheetMainPanel.Controls.Add(this.RunSheetBottomPanel);
            this.RunSheetMainPanel.Controls.Add(this.RunSheetTopPanel);
            this.RunSheetMainPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RunSheetMainPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RunSheetMainPanel.Name = "RunSheetMainPanel";
            this.RunSheetMainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 603, true);
            this.RunSheetMainPanel.TabIndex = 0;
            // 
            // RunSheetBottomPanel
            // 
            this.RunSheetBottomPanel.Controls.Add(this.InstructionsSplitPanel);
            this.RunSheetBottomPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RunSheetBottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 119, true);
            this.RunSheetBottomPanel.Name = "RunSheetBottomPanel";
            this.RunSheetBottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 484, true);
            this.RunSheetBottomPanel.TabIndex = 5;
            // 
            // InstructionsSplitPanel
            // 
            this.InstructionsSplitPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.InstructionsSplitPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.InstructionsSplitPanel.Name = "InstructionsSplitPanel";
            this.InstructionsSplitPanel.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // InstructionsSplitPanel.Panel1
            // 
            this.InstructionsSplitPanel.Panel1.Controls.Add(this.RunSheetInstructionsControl);
            this.InstructionsSplitPanel.Panel1.Controls.Add(this.ControlBoxToolStrip);
            this.InstructionsSplitPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 484, true);
            this.InstructionsSplitPanel.Panel1MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
            // 
            // InstructionsSplitPanel.Panel2
            // 
            this.InstructionsSplitPanel.Panel2.Controls.Add(this.ConsignmentsUserControl);
            this.InstructionsSplitPanel.Panel2MinSize = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(100);
            this.InstructionsSplitPanel.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiY(304);
            this.InstructionsSplitPanel.TabIndex = 4;
            // 
            // RunSheetInstructionsControl
            // 
            this.RunSheetInstructionsControl.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.RunSheetInstructionsControl, ".");
            this.RunSheetInstructionsControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.RunSheetInstructionsControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 25, true);
            this.RunSheetInstructionsControl.Name = "RunSheetInstructionsControl";
            this.RunSheetInstructionsControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 279, true);
            this.RunSheetInstructionsControl.TabIndex = 5;
            // 
            // ControlBoxToolStrip
            // 
            this.ControlBoxToolStrip.Font = new System.Drawing.Font("Segoe UI", 9F);
            this.ControlBoxToolStrip.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
            this.ControlBoxToolStrip.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.DetailsButton,
            this.toolStripSeparator2});
            this.ControlBoxToolStrip.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ControlBoxToolStrip.Name = "ControlBoxToolStrip";
            this.ControlBoxToolStrip.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 25, true);
            this.ControlBoxToolStrip.TabIndex = 4;
            // 
            // DetailsButton
            // 
            this.DetailsButton.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("f5c34450-338a-4b28-b81c-ec356ac47aaf", "Details");
            this.DetailsButton.CheckOnClick = true;
            this.DetailsButton.ImageAlign = System.Drawing.ContentAlignment.MiddleRight;
            this.DetailsButton.ImageTransparentColor = System.Drawing.Color.Magenta;
            this.DetailsButton.Name = "DetailsButton";
            this.DetailsButton.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(26, 0, 0, 0, true);
            this.DetailsButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 22, true);
            this.DetailsButton.Click += new System.EventHandler(this.DetailsButton_Click);
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(6, 25);
			// 
			// ConsignmentsUserControl
			// 
			this.ConsignmentsUserControl.AllowDrop = true;
            this.ConsignmentsUserControl.BackColor = System.Drawing.SystemColors.Control;
            this.BindingSource.SetBindingMember(this.ConsignmentsUserControl, ".");
            this.ConsignmentsUserControl.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ConsignmentsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.ConsignmentsUserControl.Name = "ConsignmentsUserControl";
            this.ConsignmentsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 176, true);
            this.ConsignmentsUserControl.TabIndex = 0;
            // 
            // RunSheetTopPanel
            // 
            this.RunSheetTopPanel.BackColor = System.Drawing.SystemColors.Control;
            this.RunSheetTopPanel.Controls.Add(this.TopGroupBox);
			this.RunSheetTopPanel.Controls.Add(this.GlowLinkLabel);
			this.RunSheetTopPanel.Dock = System.Windows.Forms.DockStyle.Top;
            this.RunSheetTopPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
            this.RunSheetTopPanel.Name = "RunSheetTopPanel";
            this.RunSheetTopPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(820, 119, true);
            this.RunSheetTopPanel.TabIndex = 4;
            // 
            // TopGroupBox
            // 
            this.TopGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.TopGroupBox.Controls.Add(this.GroupBoxSeparator);
            this.TopGroupBox.Controls.Add(this.AdHocRegoTextBox);
            this.TopGroupBox.Controls.Add(this.AdHocLicenseTextBox);
            this.TopGroupBox.Controls.Add(this.AdHocTransportCoTextBox);
            this.TopGroupBox.Controls.Add(this.RunSheetStatusTextBox);
            this.TopGroupBox.Controls.Add(this.AdHocDriverTextBox);
            this.TopGroupBox.Controls.Add(this.TransportCoFindBox);
            this.TopGroupBox.Controls.Add(this.StaffDriverFindBox);
            this.TopGroupBox.Controls.Add(this.VechicleFindBox);
            this.TopGroupBox.Controls.Add(this.EndDateEdit);
            this.TopGroupBox.Controls.Add(this.StartDateEdit);
            this.TopGroupBox.Controls.Add(this.DurationTimeEditEx);
            this.TopGroupBox.Controls.Add(this.IsHazardousCheckBox);
            this.TopGroupBox.Controls.Add(this.RequiresRefrigerationCheckBox);
            this.TopGroupBox.Enabled = false;
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.TopGroupBox, false);
            this.TopGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(1, -5, true);
            this.TopGroupBox.Name = "TopGroupBox";
            this.TopGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(817, 110, true);
            this.TopGroupBox.TabIndex = 0;
            this.TopGroupBox.TabStop = false;
            // 
            // GlowLinkLabel
            // 
            this.GlowLinkLabel.AutoSize = true;
            this.GlowLinkLabel.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("45a462bf-be9a-493c-a511-b99ec4808a53", "Open Run Sheet via the Land Transport Desktop Portal.");
            this.GlowLinkLabel.IsFontBold = false;
            this.GlowLinkLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 103, true);
            this.GlowLinkLabel.Name = "GlowLinkLabel";
            this.GlowLinkLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(275, 13, true);
            this.GlowLinkLabel.TabIndex = 9;
            this.GlowLinkLabel.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.GlowLinkLabel_LinkClicked);
            // 
            // GroupBoxSeparator
            // 
            this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.GroupBoxSeparator, false);
            this.GroupBoxSeparator.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(516, 12, true);
            this.GroupBoxSeparator.Name = "GroupBoxSeparator";
            this.GroupBoxSeparator.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(2, 92, true);
            this.GroupBoxSeparator.TabIndex = 30;
            this.GroupBoxSeparator.TabStop = false;
            // 
            // AdHocRegoTextBox
            // 
            this.BindingSource.SetBindingMember(this.AdHocRegoTextBox, "KG_AdHocTruckRegistration");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_AdHocTruckRegistration)));
            this.AdHocRegoTextBox.CaptionResourceString = null;
            this.AdHocRegoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 63, true);
            this.AdHocRegoTextBox.Name = "AdHocRegoTextBox";
            this.AdHocRegoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
            this.AdHocRegoTextBox.TabIndex = 20;
            // 
            // AdHocLicenseTextBox
            // 
            this.BindingSource.SetBindingMember(this.AdHocLicenseTextBox, "KG_AdHocDriversLicence");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_AdHocDriversLicence)));
            this.AdHocLicenseTextBox.CaptionResourceString = null;
            this.AdHocLicenseTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 40, true);
            this.AdHocLicenseTextBox.Name = "AdHocLicenseTextBox";
            this.AdHocLicenseTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
            this.AdHocLicenseTextBox.TabIndex = 19;
            // 
            // AdHocTransportCoTextBox
            // 
            this.BindingSource.SetBindingMember(this.AdHocTransportCoTextBox, "KG_AdHocTransportCoName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_AdHocTransportCoName)));
            this.AdHocTransportCoTextBox.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("7c9a765c-117e-4142-83c7-6c2f670426f5", "Transport");
            this.AdHocTransportCoTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 86, true);
            this.AdHocTransportCoTextBox.Name = "AdHocTransportCoTextBox";
            this.AdHocTransportCoTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
            this.AdHocTransportCoTextBox.TabIndex = 21;
            // 
            // RunSheetStatusTextBox
            // 
            this.BindingSource.SetBindingMember(this.RunSheetStatusTextBox, "Status");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).Status)));
            this.RunSheetStatusTextBox.CaptionResourceString = Enterprise.TransportConsignment.GUI.Res.GetData("05e76c65-8ca7-4b5b-b062-fdc1927d8e54", "Status");
            this.RunSheetStatusTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 86, true);
            this.RunSheetStatusTextBox.Name = "RunSheetStatusTextBox";
            this.RunSheetStatusTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.RunSheetStatusTextBox.TabIndex = 17;
            // 
            // AdHocDriverTextBox
            // 
            this.BindingSource.SetBindingMember(this.AdHocDriverTextBox, "KG_AdHocDriversName");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_AdHocDriversName)));
            this.AdHocDriverTextBox.CaptionResourceString = null;
            this.AdHocDriverTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(599, 17, true);
            this.AdHocDriverTextBox.Name = "AdHocDriverTextBox";
            this.AdHocDriverTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(199, 20, true);
            this.AdHocDriverTextBox.TabIndex = 18;
            // 
            // TransportCoFindBox
            // 
            this.TransportCoFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.TransportCoFindBox, "KG_OH_TransportCo");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_OH_TransportCo)));
            this.TransportCoFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 63, true);
            this.TransportCoFindBox.Name = "TransportCoFindBox";
            this.TransportCoFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.TransportCoFindBox.ParentType = null;
            this.TransportCoFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.TransportCoFindBox.TabIndex = 16;
            // 
            // StaffDriverFindBox
            // 
            this.StaffDriverFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.StaffDriverFindBox, "KG_GS_NKTruckDriver");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_GS_NKTruckDriver)));
            this.StaffDriverFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 17, true);
            this.StaffDriverFindBox.Name = "StaffDriverFindBox";
            this.StaffDriverFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.StaffDriverFindBox.ParentType = null;
            this.StaffDriverFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.StaffDriverFindBox.TabIndex = 10;
            // 
            // VechicleFindBox
            // 
            this.VechicleFindBox.AllowDrop = true;
            this.BindingSource.SetBindingMember(this.VechicleFindBox, "KG_RQ_Truck");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_RQ_Truck)));
            this.VechicleFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(256, 40, true);
            this.VechicleFindBox.Name = "VechicleFindBox";
            this.VechicleFindBox.ParentModuleID = Enterprise.ZArchitecture.Modules.ModuleIDs.NotAssigned;
            this.VechicleFindBox.ParentType = null;
            this.VechicleFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
            this.VechicleFindBox.TabIndex = 15;
            // 
            // EndDateEdit
            // 
            this.EndDateEdit.AllowDrop = true;
            this.EndDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.EndDateEdit, "KG_EndTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_EndTime)));
            this.EndDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.EndDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 40, true);
            this.EndDateEdit.Name = "EndDateEdit";
            this.EndDateEdit.TabIndex = 1;
            // 
            // StartDateEdit
            // 
            this.StartDateEdit.AllowDrop = true;
            this.StartDateEdit.AutoCompleteMonthThreshold = 1;
            this.BindingSource.SetBindingMember(this.StartDateEdit, "KG_StartTime");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_StartTime)));
            this.StartDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
            this.StartDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 17, true);
            this.StartDateEdit.Name = "StartDateEdit";
            this.StartDateEdit.TabIndex = 0;
            // 
            // DurationTimeEditEx
            // 
            this.DurationTimeEditEx.BackColor = System.Drawing.SystemColors.Window;
            this.BindingSource.SetBindingMember(this.DurationTimeEditEx, "KG_Duration");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).KG_Duration)));
            this.DurationTimeEditEx.CaptionResourceString = null;
            this.DurationTimeEditEx.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(63, 63, true);
            this.DurationTimeEditEx.Name = "DurationTimeEditEx";
            this.DurationTimeEditEx.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(42, 20, true);
            this.DurationTimeEditEx.TabIndex = 3;
            // 
            // IsHazardousCheckBox
            // 
            this.BindingSource.SetBindingMember(this.IsHazardousCheckBox, "IsHazardous");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).IsHazardous)));
            this.IsHazardousCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(20, 88, true);
            this.IsHazardousCheckBox.Name = "IsHazardousCheckBox";
            this.IsHazardousCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(78, 17, true);
            this.IsHazardousCheckBox.TabIndex = 31;
            this.IsHazardousCheckBox.UseVisualStyleBackColor = true;
            // 
            // RequiresRefrigerationCheckBox
            // 
            this.BindingSource.SetBindingMember(this.RequiresRefrigerationCheckBox, "RequiresRefrigeration");
            // The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
            CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet)(null)).RequiresRefrigeration)));
            this.RequiresRefrigerationCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(105, 88, true);
            this.RequiresRefrigerationCheckBox.Name = "RequiresRefrigerationCheckBox";
            this.RequiresRefrigerationCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(90, 17, true);
            this.RequiresRefrigerationCheckBox.TabIndex = 32;
            this.RequiresRefrigerationCheckBox.UseVisualStyleBackColor = true;
            // 
            // WorkflowTabPage
            // 
            this.WorkflowTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
            this.WorkflowTabPage.Name = "WorkflowTabPage";
            this.WorkflowTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
            this.WorkflowTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(842, 550, true);
            this.WorkflowTabPage.TabIndex = 3;
            this.WorkflowTabPage.UseVisualStyleBackColor = true;
            // 
            // DtbConsignmentRunSheetForm
            // 
            this.CaptionRenderingEnabled = true;
            this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(828, 686, true);
            this.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignmentRunSheet);
            this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(836, 725, true);
            this.Name = "DtbConsignmentRunSheetForm";
            this.ShouldSerializeTabPageMethods = false;
            this.MainTabControl.ResumeLayout(false);
            this.MainTabControl.PerformLayout();
            this.MainTabPage.ResumeLayout(false);
            this.MainTabPage.PerformLayout();
            this.NotesTabPage.ResumeLayout(false);
            this.NotesTabPage.PerformLayout();
            this.MainPanel.ResumeLayout(false);
            this.MainPanel.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
            this.RunSheetMainPanel.ResumeLayout(false);
            this.RunSheetMainPanel.PerformLayout();
            this.RunSheetBottomPanel.ResumeLayout(false);
            this.RunSheetBottomPanel.PerformLayout();
            this.InstructionsSplitPanel.Panel1.ResumeLayout(false);
            this.InstructionsSplitPanel.Panel1.PerformLayout();
            this.InstructionsSplitPanel.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.InstructionsSplitPanel)).EndInit();
            this.InstructionsSplitPanel.ResumeLayout(false);
            this.InstructionsSplitPanel.PerformLayout();
            this.RunSheetInstructionsControl.ResumeLayout(true);
            this.RunSheetInstructionsControl.PerformLayout();
            this.ControlBoxToolStrip.ResumeLayout(false);
            this.ControlBoxToolStrip.PerformLayout();
            this.ConsignmentsUserControl.ResumeLayout(true);
            this.ConsignmentsUserControl.PerformLayout();
            this.RunSheetTopPanel.ResumeLayout(false);
            this.RunSheetTopPanel.PerformLayout();
            this.TopGroupBox.ResumeLayout(false);
            this.TopGroupBox.PerformLayout();
            this.TransportCoFindBox.ResumeLayout(true);
            this.TransportCoFindBox.PerformLayout();
            this.StaffDriverFindBox.ResumeLayout(true);
            this.StaffDriverFindBox.PerformLayout();
            this.VechicleFindBox.ResumeLayout(true);
            this.VechicleFindBox.PerformLayout();
            this.EndDateEdit.ResumeLayout(true);
            this.EndDateEdit.PerformLayout();
            this.StartDateEdit.ResumeLayout(true);
            this.StartDateEdit.PerformLayout();
            this.WorkflowTabPage.ResumeLayout(false);
            this.WorkflowTabPage.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel RunSheetMainPanel;
		private ZArchitecture.GUI.ZPanel RunSheetBottomPanel;
		private CargoWise.Windows.UI.KSplitContainer InstructionsSplitPanel;
		private ZArchitecture.GUI.ZToolStrip ControlBoxToolStrip;
		private ZArchitecture.GUI.ZToolStripButton DetailsButton;
		private DtbConsignmentRunSheetDetailsUserControl ConsignmentsUserControl;
		private ZArchitecture.GUI.ZPanel RunSheetTopPanel;
		private ZGroupBoxWithoutCaption TopGroupBox;
		private ZArchitecture.GUI.ZDateEdit EndDateEdit;
		private ZArchitecture.GUI.ZDateEdit StartDateEdit;
		private ZArchitecture.GUI.ZGuidFindBox VechicleFindBox;
		private ZArchitecture.GUI.ZCodeFindBox StaffDriverFindBox;
		private ZArchitecture.GUI.ZGuidFindBox TransportCoFindBox;
		private ZArchitecture.ZTextBox RunSheetStatusTextBox;
		private ZArchitecture.ZTextBox AdHocDriverTextBox;
		private ZArchitecture.ZTextBox AdHocLicenseTextBox;
		private ZArchitecture.ZTextBox AdHocTransportCoTextBox;
		private ZArchitecture.ZTextBox AdHocRegoTextBox;
		private ZGroupBoxWithoutCaption GroupBoxSeparator;
		private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
		private DtbConsignmentRunSheetInstructionsUserControl RunSheetInstructionsControl;
		private Enterprise.ZArchitecture.GUI.ZTimeEditEx DurationTimeEditEx;
		private ZCheckBox IsHazardousCheckBox;
		private ZCheckBox RequiresRefrigerationCheckBox;
        private Enterprise.MasterFiles.GUI.ZWorkflowTabPage WorkflowTabPage;
		public ZLinkLabel GlowLinkLabel;
	}
}
