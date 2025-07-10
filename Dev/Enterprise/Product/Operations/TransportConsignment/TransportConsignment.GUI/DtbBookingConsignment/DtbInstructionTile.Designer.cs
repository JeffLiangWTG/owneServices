using Enterprise.MasterFiles.GUI;

namespace Enterprise.TransportConsignment.GUI
{
	partial class DtbInstructionTile
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
			this.IsAuthorisedToLeaveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.zGroupBox1 = new Enterprise.TransportConsignment.GUI.ZGroupBoxWithoutCaption();
			this.DocAddressControl = new Enterprise.MasterFiles.GUI.ZDocAddressControl();
			this.DatesAndInstructionsPanel = new Enterprise.TransportConsignment.GUI.DtbInstructionTile.ZPanelThatOverlapsDocAddress();
			this.DropModeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.InstructionsTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.RequiredToDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.RequiredFromDateEdit = new Enterprise.ZArchitecture.GUI.ZDateEdit();
			this.ZoneLabel = new Enterprise.TransportConsignment.GUI.DtbInstructionTile.ZLabelThatOverlapsDocAddress();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.DatesAndInstructionsPanel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.TransportConsignment.Business.DtbConsignmentInstruction);
			// 
			// IsAuthorisedToLeaveCheckBox
			// 
			this.BindingSource.SetBindingMember(this.IsAuthorisedToLeaveCheckBox, "KN_IsAuthorisedToLeave");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.TransportConsignment.Business.DtbConsignmentInstruction)(null)).KN_IsAuthorisedToLeave)));			
			this.IsAuthorisedToLeaveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsAuthorisedToLeaveCheckBox.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.IsAuthorisedToLeaveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 83, true);
			this.IsAuthorisedToLeaveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(140, 17, true);
			this.IsAuthorisedToLeaveCheckBox.Name = "IsAuthorisedToLeaveCheckBox";
			this.IsAuthorisedToLeaveCheckBox.TabIndex = 11;
			// 
			// zGroupBox1
			// 
			this.zGroupBox1.Controls.Add(this.ZoneLabel);
			this.zGroupBox1.Controls.Add(this.DatesAndInstructionsPanel);
			this.zGroupBox1.Controls.Add(this.DocAddressControl);
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.zGroupBox1, false);
			this.zGroupBox1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.zGroupBox1.Name = "zGroupBox1";
			this.zGroupBox1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(510, 190, true);
			this.zGroupBox1.TabIndex = 10;
			this.zGroupBox1.TabStop = false;
			// 
			// DocAddressControl
			// 
			this.DocAddressControl.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DocAddressControl, "Address");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Enterprise.MasterFiles.Business.JobDocAddress)(((Enterprise.TransportConsignment.Business.DtbConsignmentInstruction)(null)).Address)));
			this.DocAddressControl.CaptionResourceString = null;
			this.DocAddressControl.IsCustomHeight = true;
			this.DocAddressControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.DocAddressControl.Name = "DocAddressControl";
			this.DocAddressControl.SingleLineNoGroupBoxPanelWidth = 296;
			this.DocAddressControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 185, true);
			this.DocAddressControl.TabIndex = 1;
			// 
			// DatesAndInstructionsPanel
			// 
			this.DatesAndInstructionsPanel.Controls.Add(this.DropModeDropEdit);
			this.DatesAndInstructionsPanel.Controls.Add(this.InstructionsTextBox);
			this.DatesAndInstructionsPanel.Controls.Add(this.IsAuthorisedToLeaveCheckBox);
			this.DatesAndInstructionsPanel.Controls.Add(this.RequiredToDateEdit);
			this.DatesAndInstructionsPanel.Controls.Add(this.RequiredFromDateEdit);
			this.DatesAndInstructionsPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(255, 12, true);
			this.DatesAndInstructionsPanel.Name = "DatesAndInstructionsPanel";
			this.DatesAndInstructionsPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(249, 172, true);
			this.DatesAndInstructionsPanel.TabIndex = 8;
			// 
			// DropModeDropEdit
			// 
			this.DropModeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.DropModeDropEdit, "KN_DropMode");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportConsignment.Business.DtbConsignmentInstruction)(null)).KN_DropMode)));
			this.DropModeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 57, true);
			this.DropModeDropEdit.Name = "DropModeDropEdit";
			this.DropModeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 20, true);
			this.DropModeDropEdit.TabIndex = 10;
			// 
			// InstructionsTextBox
			// 
			this.BindingSource.SetBindingMember(this.InstructionsTextBox, "KN_ServiceInstruction");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.TransportConsignment.Business.DtbConsignmentInstruction)(null)).KN_ServiceInstruction)));
			this.InstructionsTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 102, true);
			this.InstructionsTextBox.Multiline = true;
			this.InstructionsTextBox.Name = "InstructionsTextBox";
			this.InstructionsTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 69, true);
			this.InstructionsTextBox.TabIndex = 12;
			// 
			// RequiredToDateEdit
			// 
			this.RequiredToDateEdit.AllowDrop = true;
			this.RequiredToDateEdit.AutoCompleteMonthThreshold = 1;
			this.RequiredToDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RequiredToDateEdit, "ReqTo");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportConsignment.Business.DtbConsignmentInstruction)(null)).ReqTo)));
			this.RequiredToDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RequiredToDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 31, true);
			this.RequiredToDateEdit.Name = "RequiredToDateEdit";
			this.RequiredToDateEdit.TabIndex = 9;
			// 
			// RequiredFromDateEdit
			// 
			this.RequiredFromDateEdit.AllowDrop = true;
			this.RequiredFromDateEdit.AutoCompleteMonthThreshold = 1;
			this.RequiredFromDateEdit.AutoCompleteYear = true;
			this.BindingSource.SetBindingMember(this.RequiredFromDateEdit, "ReqFrom");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.TransportConsignment.Business.DtbConsignmentInstruction)(null)).ReqFrom)));
			this.RequiredFromDateEdit.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Long;
			this.RequiredFromDateEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 5, true);
			this.RequiredFromDateEdit.Name = "RequiredFromDateEdit";
			this.RequiredFromDateEdit.TabIndex = 8;
			// 
			// ZoneLabel
			// 
			this.ZoneLabel.AutoSize = true;
			this.ZoneLabel.ForeColor = System.Drawing.SystemColors.ControlDarkDark;
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ZoneLabel, false);
			this.ZoneLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(125, 1, true);
			this.ZoneLabel.Name = "ZoneLabel";
			this.ZoneLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(0, 13, true);
			this.ZoneLabel.TabIndex = 9;
			// 
			// DtbInstructionTile
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.zGroupBox1);
			this.Name = "DtbInstructionTile";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(513, 194, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.zGroupBox1.ResumeLayout(false);
			this.zGroupBox1.PerformLayout();
			this.DatesAndInstructionsPanel.ResumeLayout(false);
			this.DatesAndInstructionsPanel.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZGroupBoxWithoutCaption zGroupBox1;
		public ZDocAddressControl DocAddressControl;
		private ZPanelThatOverlapsDocAddress DatesAndInstructionsPanel;
		private ZArchitecture.ZTextBox InstructionsTextBox;
		private ZArchitecture.GUI.ZDateEdit RequiredToDateEdit;
		private ZArchitecture.GUI.ZDateEdit RequiredFromDateEdit;
		private ZArchitecture.GUI.ZCheckBox IsAuthorisedToLeaveCheckBox;
		protected ZLabelThatOverlapsDocAddress ZoneLabel;
		private ZArchitecture.GUI.ZDropEdit DropModeDropEdit;
	}
}
