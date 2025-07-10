using Enterprise.Freight.LocalCartage.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.LocalCartage.GUI
{
	partial class RunSheetDashboardControl
	{
		/// <summary> 
		/// Required designer variable.
		/// </summary>
		private System.ComponentModel.IContainer components = null;


		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			this.OptionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.OptionsGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.RefreshPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.AutoRefreshCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.RefreshButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.zPanel1 = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.BranchGuidFindBox = new Enterprise.ZArchitecture.GUI.ZGuidFindBox();
			this.DateRangePanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.DateRangeToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateRangeFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.DateOptionsPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.WeekdayDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.BodyPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.lblErrorMessage = new Enterprise.ZArchitecture.ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.OptionsPanel.SuspendLayout();
			this.OptionsGroupBox.SuspendLayout();
			this.RefreshPanel.SuspendLayout();
			this.zPanel1.SuspendLayout();
			this.DateRangePanel.SuspendLayout();
			this.DateOptionsPanel.SuspendLayout();
			this.BodyPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Freight.LocalCartage.Business.RunSheetDashboard);
			// 
			// OptionsPanel
			// 
			this.OptionsPanel.Controls.Add(this.OptionsGroupBox);
			this.OptionsPanel.Dock = System.Windows.Forms.DockStyle.Top;
			this.OptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionsPanel.Name = "OptionsPanel";
			this.OptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 40, true);
			this.OptionsPanel.TabIndex = 1;
			// 
			// OptionsGroupBox
			// 
			this.OptionsGroupBox.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("RunSheetDashboardControl|824edbb6-bd02-4d3c-95d0-324e9e9320dc", "Options");
			this.OptionsGroupBox.Controls.Add(this.RefreshPanel);
			this.OptionsGroupBox.Controls.Add(this.zPanel1);
			this.OptionsGroupBox.Controls.Add(this.DateRangePanel);
			this.OptionsGroupBox.Controls.Add(this.DateOptionsPanel);
			this.OptionsGroupBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.OptionsGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.OptionsGroupBox.Name = "OptionsGroupBox";
			this.OptionsGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 40, true);
			this.OptionsGroupBox.TabIndex = 0;
			this.OptionsGroupBox.TabStop = false;
			// 
			// RefreshPanel
			// 
			this.RefreshPanel.Controls.Add(this.AutoRefreshCheckBox);
			this.RefreshPanel.Controls.Add(this.RefreshButton);
			this.RefreshPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RefreshPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(592, 16, true);
			this.RefreshPanel.Name = "RefreshPanel";
			this.RefreshPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(540, 21, true);
			this.RefreshPanel.TabIndex = 3;
			// 
			// AutoRefreshCheckBox
			// 
			this.AutoRefreshCheckBox.AutoSize = true;
			this.AutoRefreshCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.AutoRefreshCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(6, 3, true);
			this.AutoRefreshCheckBox.Name = "AutoRefreshCheckBox";
			this.AutoRefreshCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.AutoRefreshCheckBox.TabIndex = 0;
			this.AutoRefreshCheckBox.UseVisualStyleBackColor = true;
			// 
			// RefreshButton
			// 
			this.RefreshButton.CaptionResourceString = Enterprise.Freight.LocalCartage.GUI.Res.GetData("RunSheetDashboardControl|3443d15a-0ead-4288-8b77-6970bea11a21", "Refresh", "Refresh Run Sheet Dashboard.");
			this.RefreshButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(203, 0, true);
			this.RefreshButton.Name = "RefreshButton";
			this.RefreshButton.AutoSize = true;
			this.RefreshButton.MaximumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 21, true); ;
			this.RefreshButton.TabIndex = 1;
			this.RefreshButton.UseVisualStyleBackColor = true;
			this.RefreshButton.Click += new System.EventHandler(this.RefreshButton_Click);
			// 
			// zPanel1
			// 
			this.zPanel1.Controls.Add(this.BranchGuidFindBox);
			this.zPanel1.Dock = System.Windows.Forms.DockStyle.Left;
			this.zPanel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(368, 16, true);
			this.zPanel1.Name = "zPanel1";
			this.zPanel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(224, 21, true);
			this.zPanel1.TabIndex = 2;
			// 
			// BranchGuidFindBox
			// 
			this.BranchGuidFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.BranchGuidFindBox, "Branch");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZGuid)(((Enterprise.Freight.LocalCartage.Business.RunSheetDashboard)(null)).Branch)));
			this.BranchGuidFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(50, 1, true);
			this.BranchGuidFindBox.Name = "BranchGuidFindBox";
			this.BranchGuidFindBox.PreBoundMaxLength = 5;
			this.BranchGuidFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(168, 20, true);
			this.BranchGuidFindBox.TabIndex = 3;
			// 
			// DateRangePanel
			// 
			this.DateRangePanel.Controls.Add(this.DateRangeToDateEdit);
			this.DateRangePanel.Controls.Add(this.DateRangeFromDateEdit);
			this.DateRangePanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.DateRangePanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(187, 16, true);
			this.DateRangePanel.Name = "DateRangePanel";
			this.DateRangePanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(181, 21, true);
			this.DateRangePanel.TabIndex = 1;
			// 
			// DateRangeToDateEdit
			// 
			this.DateRangeToDateEdit.AllowDrop = true;
			this.DateRangeToDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateRangeToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateRangeToDateEdit, "DateRangeTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.RunSheetDashboard)(null)).DateRangeTo)));
			this.DateRangeToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(92, 1, true);
			this.DateRangeToDateEdit.Name = "DateRangeToDateEdit";
			this.DateRangeToDateEdit.TabIndex = 2;
			// 
			// DateRangeFromDateEdit
			// 
			this.DateRangeFromDateEdit.AllowDrop = true;
			this.DateRangeFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.DateRangeFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.DateRangeFromDateEdit, "DateRangeFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.RunSheetDashboard)(null)).DateRangeFrom)));
			this.DateRangeFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 1, true);
			this.DateRangeFromDateEdit.Name = "DateRangeFromDateEdit";
			this.DateRangeFromDateEdit.TabIndex = 1;
			// 
			// DateOptionsPanel
			// 
			this.DateOptionsPanel.Controls.Add(this.WeekdayDropEdit);
			this.DateOptionsPanel.Dock = System.Windows.Forms.DockStyle.Left;
			this.DateOptionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 16, true);
			this.DateOptionsPanel.Name = "DateOptionsPanel";
			this.DateOptionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(184, 21, true);
			this.DateOptionsPanel.TabIndex = 0;
			// 
			// WeekdayDropEdit
			// 
			this.WeekdayDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.WeekdayDropEdit, "DateRangeFilter");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Freight.LocalCartage.Business.RunSheetDashboard)(null)).DateRangeFilter)));
			this.WeekdayDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.WeekdayDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(73, 1, true);
			this.WeekdayDropEdit.Name = "WeekdayDropEdit";
			this.WeekdayDropEdit.ShowDescriptionBox = false;
			this.WeekdayDropEdit.ShowInDropDown = Enterprise.ZArchitecture.GUI.ZDropEdit.ShowInDropDownList.OnlyShowCode;
			this.WeekdayDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(104, 20, true);
			this.WeekdayDropEdit.TabIndex = 0;
			// 
			// BodyPanel
			// 
			this.BodyPanel.AutoScroll = true;
			this.BodyPanel.Controls.Add(this.lblErrorMessage);
			this.BodyPanel.Dock = System.Windows.Forms.DockStyle.Fill;
			this.BodyPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.BodyPanel.Name = "BodyPanel";
			this.BodyPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 462, true);
			this.BodyPanel.TabIndex = 0;
			// 
			// lblErrorMessage
			// 
			this.BindingSource.SetBindingMember(this.lblErrorMessage, "ErrorMessage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Freight.LocalCartage.Business.RunSheetDashboard)(null)).ErrorMessage)));
			this.lblErrorMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblErrorMessage.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.lblErrorMessage.Name = "lblErrorMessage";
			this.lblErrorMessage.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 462, true);
			this.lblErrorMessage.TabIndex = 0;
			this.lblErrorMessage.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
			// 
			// RunSheetDashboardControl
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.BodyPanel);
			this.Controls.Add(this.OptionsPanel);
			this.Name = "RunSheetDashboardControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1135, 502, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.OptionsPanel.ResumeLayout(false);
			this.OptionsGroupBox.ResumeLayout(false);
			this.RefreshPanel.ResumeLayout(false);
			this.RefreshPanel.PerformLayout();
			this.zPanel1.ResumeLayout(false);
			this.DateRangePanel.ResumeLayout(false);
			this.DateOptionsPanel.ResumeLayout(false);
			this.BodyPanel.ResumeLayout(false);
			this.ResumeLayout(false);

		}

		#endregion

		private Enterprise.ZArchitecture.GUI.ZPanel OptionsPanel;
		private Enterprise.ZArchitecture.GUI.ZCheckBox AutoRefreshCheckBox;
		internal Enterprise.ZArchitecture.GUI.ZButton RefreshButton;
		private Enterprise.ZArchitecture.GUI.ZDropEdit WeekdayDropEdit;
		internal ZPanel BodyPanel;
		private Enterprise.ZArchitecture.GUI.ZGroupBox OptionsGroupBox;
		internal ZPanel RefreshPanel;
		internal ZPanel DateOptionsPanel;
		internal ZDateEdit DateRangeToDateEdit;
		internal ZDateEdit DateRangeFromDateEdit;
		private ZGuidFindBox BranchGuidFindBox;
        private ZPanel DateRangePanel;
        private ZPanel zPanel1;
        private Enterprise.ZArchitecture.ZLabel lblErrorMessage;
	}
}
