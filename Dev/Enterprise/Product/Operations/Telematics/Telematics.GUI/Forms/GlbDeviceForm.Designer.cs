namespace Enterprise.Telematics.GUI.Forms
{
	partial class GlbDeviceForm
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
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		#region Windows Form Designer generated code

		/// <summary>
		/// Required method for Designer support - do not modify
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.components = new System.ComponentModel.Container();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			this.humanReadableNameTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.deviceInformationGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.isActiveCheckbox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.modelTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.tabControl = new Enterprise.ZArchitecture.GUI.ZTabControl();
			this.locationInfoTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.locationInfoLabel = new Enterprise.ZArchitecture.ZLabel();
			this.assignmentHistoryTabPage = new Enterprise.ZArchitecture.GUI.ZTabPage();
			this.assignmentHistoryDataGrid = new Enterprise.ZArchitecture.ZGrid();
			this.assignmentGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.clearAssignmentButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.parentStaffFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.parentEquipmentFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.locationInfoTimer = new System.Windows.Forms.Timer(this.components);
			this.MainTabControl.SuspendLayout();
			this.MainTabPage.SuspendLayout();
			this.NotesTabPage.SuspendLayout();
			this.MainPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.deviceInformationGroupBox.SuspendLayout();
			this.tabControl.SuspendLayout();
			this.locationInfoTabPage.SuspendLayout();
			this.assignmentHistoryTabPage.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.assignmentHistoryDataGrid)).BeginInit();
			this.assignmentHistoryDataGrid.SuspendLayout();
			this.assignmentGroupBox.SuspendLayout();
			this.parentStaffFindBox.SuspendLayout();
			this.parentEquipmentFindBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainTabControl
			// 
			this.MainTabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 487, true);
			this.MainTabControl.TabIndex = 0;
			// 
			// MainTabPage
			// 
			this.MainTabPage.Controls.Add(this.tabControl);
			this.MainTabPage.Controls.Add(this.assignmentGroupBox);
			this.MainTabPage.Controls.Add(this.deviceInformationGroupBox);
			this.MainTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 460, true);
			// 
			// NotesTabPage
			// 
			this.NotesTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 460, true);
			// 
			// LogsTabPage
			// 
			this.LogsTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(772, 460, true);
			// 
			// MainPanel
			// 
			this.MainPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 487, true);
			// 
			// PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel
			// 
			this.PlaceButtonsFromDescendantFormsOnThisPanelSoThatSaveControlDoesntJumpAroundPanel.TabIndex = 0;
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 24, true);
			this.MainStatusBar.TabIndex = 4;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Telematics.Business.GlbDevice);
			// 
			// humanReadableNameTextBox
			// 
			this.humanReadableNameTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.humanReadableNameTextBox, "V3_HumanReadableIdentifier");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.GlbDevice)(null)).V3_HumanReadableIdentifier)));
			this.humanReadableNameTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.humanReadableNameTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 19, true);
			this.humanReadableNameTextBox.Name = "humanReadableNameTextBox";
			this.humanReadableNameTextBox.ReadOnly = true;
			this.humanReadableNameTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 20, true);
			this.humanReadableNameTextBox.TabIndex = 0;
			// 
			// deviceInformationGroupBox
			// 
			this.deviceInformationGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.deviceInformationGroupBox.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("cce4b756-2c62-4726-ab62-77ad4cc59899", "Basic Details");
			this.deviceInformationGroupBox.Controls.Add(this.isActiveCheckbox);
			this.deviceInformationGroupBox.Controls.Add(this.modelTextBox);
			this.deviceInformationGroupBox.Controls.Add(this.humanReadableNameTextBox);
			this.deviceInformationGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 9, true);
			this.deviceInformationGroupBox.Name = "deviceInformationGroupBox";
			this.deviceInformationGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 72, true);
			this.deviceInformationGroupBox.TabIndex = 0;
			this.deviceInformationGroupBox.TabStop = false;
			// 
			// isActiveCheckbox
			// 
			this.isActiveCheckbox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.isActiveCheckbox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.isActiveCheckbox, "V3_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Telematics.Business.GlbDevice)(null)).V3_IsActive)));
			this.isActiveCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.isActiveCheckbox.ForeColor = System.Drawing.SystemColors.GrayText;
			this.isActiveCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(704, 21, true);
			this.isActiveCheckbox.Name = "isActiveCheckbox";
			this.isActiveCheckbox.ReadOnly = true;
			this.isActiveCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(56, 17, true);
			this.isActiveCheckbox.TabIndex = 3;
			this.isActiveCheckbox.UseVisualStyleBackColor = true;
			// 
			// modelTextBox
			// 
			this.modelTextBox.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.modelTextBox, "V3_Model");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.GlbDevice)(null)).V3_Model)));
			this.modelTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.modelTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(126, 46, true);
			this.modelTextBox.Name = "modelTextBox";
			this.modelTextBox.ReadOnly = true;
			this.modelTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(560, 20, true);
			this.modelTextBox.TabIndex = 2;
			// 
			// tabControl
			// 
			this.tabControl.Controls.Add(this.locationInfoTabPage);
			this.tabControl.Controls.Add(this.assignmentHistoryTabPage);
			this.tabControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 164, true);
			this.tabControl.Name = "tabControl";
			this.tabControl.SelectedIndex = 0;
			this.tabControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 293, true);
			this.tabControl.TabIndex = 2;
			// 
			// locationInfoTabPage
			// 
			this.locationInfoTabPage.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("8b4e3803-c757-40ee-85bb-4d182d542a95", "Location Info");
			this.locationInfoTabPage.Controls.Add(this.locationInfoLabel);
			this.locationInfoTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.locationInfoTabPage.Name = "locationInfoTabPage";
			this.locationInfoTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.locationInfoTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 266, true);
			this.locationInfoTabPage.TabIndex = 1;
			// 
			// locationInfoLabel
			// 
			this.locationInfoLabel.AllowDrop = true;
			this.locationInfoLabel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.locationInfoLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.locationInfoLabel.Name = "locationInfoLabel";
			this.locationInfoLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(752, 260, true);
			this.locationInfoLabel.TabIndex = 1;
			this.locationInfoLabel.Text = "This device was last seen 3 days ago at -31.00022, 151.234000";
			this.locationInfoLabel.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// assignmentHistoryTabPage
			// 
			this.assignmentHistoryTabPage.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("3e06f73f-4912-493a-b4dd-dc76bb46619a", "Assignment History");
			this.assignmentHistoryTabPage.Controls.Add(this.assignmentHistoryDataGrid);
			this.assignmentHistoryTabPage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(4, 23, true);
			this.assignmentHistoryTabPage.Name = "assignmentHistoryTabPage";
			this.assignmentHistoryTabPage.Padding = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPadding(3, true);
			this.assignmentHistoryTabPage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(758, 266, true);
			this.assignmentHistoryTabPage.TabIndex = 0;
			this.assignmentHistoryTabPage.UseVisualStyleBackColor = true;
			// 
			// assignmentHistoryDataGrid
			// 
			this.assignmentHistoryDataGrid.AllowNavigation = false;
			this.assignmentHistoryDataGrid.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.assignmentHistoryDataGrid, "Assignments");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((System.Collections.IList)(((Enterprise.Telematics.Business.GlbDevice)(null)).Assignments)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Telematics.Business.GlbDeviceAssignmentDivot)(((System.Collections.IList)(((Enterprise.Telematics.Business.GlbDevice)(null)).Assignments)).SyncRoot)).ParentObjectName)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Telematics.Business.GlbDeviceAssignmentDivot)(((System.Collections.IList)(((Enterprise.Telematics.Business.GlbDevice)(null)).Assignments)).SyncRoot)).V7_StartTimeUtc)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDateTime)(((Enterprise.Telematics.Business.GlbDeviceAssignmentDivot)(((System.Collections.IList)(((Enterprise.Telematics.Business.GlbDevice)(null)).Assignments)).SyncRoot)).V7_EndTimeUtc)));
			this.assignmentHistoryDataGrid.CaptionVisible = false;
			zTextBoxColumnStyleInfo1.ColumnName = "ParentObjectName";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(180);
			zDateEditColumnStyleInfo1.ColumnName = "V7_StartTimeUtc";
			zDateEditColumnStyleInfo1.IsReadOnly = true;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDateEditColumnStyleInfo2.ColumnName = "V7_EndTimeUtc";
			zDateEditColumnStyleInfo2.IsReadOnly = true;
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			this.assignmentHistoryDataGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.assignmentHistoryDataGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.assignmentHistoryDataGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.assignmentHistoryDataGrid.GridId = "94945928-91af-43a7-89b5-4ef61e85e902";
			this.assignmentHistoryDataGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.assignmentHistoryDataGrid.LayoutKey = "assignmentHistoryDataGrid";
			this.assignmentHistoryDataGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 6, true);
			this.assignmentHistoryDataGrid.Name = "assignmentHistoryDataGrid";
			this.assignmentHistoryDataGrid.ReadOnly = true;
			this.assignmentHistoryDataGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(746, 257, true);
			this.assignmentHistoryDataGrid.TabIndex = 0;
			// 
			// assignmentGroupBox
			// 
			this.assignmentGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.assignmentGroupBox.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("6bed240a-a5da-4005-995f-351d12f71593", "Assignment");
			this.assignmentGroupBox.Controls.Add(this.clearAssignmentButton);
			this.assignmentGroupBox.Controls.Add(this.parentStaffFindBox);
			this.assignmentGroupBox.Controls.Add(this.parentEquipmentFindBox);
			this.assignmentGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 87, true);
			this.assignmentGroupBox.Name = "assignmentGroupBox";
			this.assignmentGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(766, 71, true);
			this.assignmentGroupBox.TabIndex = 1;
			this.assignmentGroupBox.TabStop = false;
			// 
			// clearAssignmentButton
			//
			this.clearAssignmentButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
			this.clearAssignmentButton.CaptionResourceString = Enterprise.Telematics.GUI.Res.GetData("03807417-0352-4fe0-acfb-c31a7fd3cb63", "Clear");
			this.clearAssignmentButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(701, 19, true);
			this.clearAssignmentButton.Name = "clearAssignmentButton";
			this.clearAssignmentButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.clearAssignmentButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(55, 46, true);
			this.clearAssignmentButton.TabIndex = 2;
			this.clearAssignmentButton.UseVisualStyleBackColor = true;
			this.clearAssignmentButton.Click += new System.EventHandler(this.OnClearAssignmentButtonClick);
			// 
			// parentStaffFindBox
			// 
			this.parentStaffFindBox.AllowDrop = true;
			this.parentStaffFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.parentStaffFindBox, "AssignedParentStaffID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Telematics.Business.GlbDevice)(null)).AssignedParentStaffID)));
			this.parentStaffFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 19, true);
			this.parentStaffFindBox.Name = "parentStaffFindBox";
			this.parentStaffFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 20, true);
			this.parentStaffFindBox.TabIndex = 0;
			// 
			// parentEquipmentFindBox
			// 
			this.parentEquipmentFindBox.AllowDrop = true;
			this.parentEquipmentFindBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.BindingSource.SetBindingMember(this.parentEquipmentFindBox, "AssignedParentEquipmentID");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Telematics.Business.GlbDevice)(null)).AssignedParentEquipmentID)));
			this.parentEquipmentFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 45, true);
			this.parentEquipmentFindBox.Name = "parentEquipmentFindBox";
			this.parentEquipmentFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(570, 20, true);
			this.parentEquipmentFindBox.TabIndex = 1;
			// 
			// locationInfoTimer
			// 
			this.locationInfoTimer.Interval = 1000;
			this.locationInfoTimer.Tick += new System.EventHandler(this.OnLocationInfoTimerTick);
			// 
			// GlbDeviceForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(780, 543, true);
			this.DataSourceType = typeof(Enterprise.Telematics.Business.GlbDevice);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(720, 500, true);
			this.Name = "GlbDeviceForm";
			this.ShouldSerializeTabPageMethods = false;
			this.Text = "";
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
			this.deviceInformationGroupBox.ResumeLayout(false);
			this.deviceInformationGroupBox.PerformLayout();
			this.tabControl.ResumeLayout(false);
			this.tabControl.PerformLayout();
			this.locationInfoTabPage.ResumeLayout(false);
			this.locationInfoTabPage.PerformLayout();
			this.assignmentHistoryTabPage.ResumeLayout(false);
			this.assignmentHistoryTabPage.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.assignmentHistoryDataGrid)).EndInit();
			this.assignmentHistoryDataGrid.ResumeLayout(false);
			this.assignmentHistoryDataGrid.PerformLayout();
			this.assignmentGroupBox.ResumeLayout(false);
			this.assignmentGroupBox.PerformLayout();
			this.parentStaffFindBox.ResumeLayout(true);
			this.parentStaffFindBox.PerformLayout();
			this.parentEquipmentFindBox.ResumeLayout(true);
			this.parentEquipmentFindBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox humanReadableNameTextBox;
		private ZArchitecture.GUI.ZGroupBox deviceInformationGroupBox;
		private ZArchitecture.GUI.ZCheckBox isActiveCheckbox;
		private ZArchitecture.ZTextBox modelTextBox;
		private ZArchitecture.GUI.ZTabControl tabControl;
		private ZArchitecture.GUI.ZTabPage assignmentHistoryTabPage;
		private ZArchitecture.GUI.ZTabPage locationInfoTabPage;
		private ZArchitecture.ZGrid assignmentHistoryDataGrid;
		private ZArchitecture.GUI.ZGroupBox assignmentGroupBox;
		private ZArchitecture.GUI.ZGuidFindBox parentEquipmentFindBox;
		private ZArchitecture.GUI.ZGuidFindBox parentStaffFindBox;
		private ZArchitecture.GUI.ZButton clearAssignmentButton;
		private ZArchitecture.ZLabel locationInfoLabel;
		private System.Windows.Forms.Timer locationInfoTimer;
	}
}
