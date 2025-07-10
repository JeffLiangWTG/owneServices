namespace Enterprise.MasterFiles.GUI
{
	partial class ViewStmNumsEditorForm
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

		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private new void InitializeComponent()
		{
			this.BottomPanel = new Enterprise.ZArchitecture.GUI.ZPanel();
			this.ValidateAndSaveButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelAndCloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.TypeDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.PrefixTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.ValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MinimumValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.CountCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.MaximumValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.BottomPanel.SuspendLayout();
			this.TypeDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 199, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ViewStmNums);
			// 
			// BottomPanel
			// 
			this.BottomPanel.Controls.Add(this.ValidateAndSaveButton);
			this.BottomPanel.Controls.Add(this.CancelAndCloseButton);
			this.BottomPanel.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.BottomPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 170, true);
			this.BottomPanel.Name = "BottomPanel";
			this.BottomPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 30, true);
			this.BottomPanel.TabIndex = 1;
			// 
			// ValidateAndSaveButton
			// 
			this.ValidateAndSaveButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("5d6afacb-0973-4695-b0cf-794942ce2235", "Save && Close");
			this.ValidateAndSaveButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(121, 3, true);
			this.ValidateAndSaveButton.Name = "ValidateAndSaveButton";
			this.ValidateAndSaveButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.ValidateAndSaveButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(150, 23, true);
			this.ValidateAndSaveButton.TabIndex = 0;
			this.ValidateAndSaveButton.UseVisualStyleBackColor = true;
			this.ValidateAndSaveButton.Click += new System.EventHandler(this.ValidateAndSaveButton_Click);
			// 
			// CancelAndCloseButton
			// 
			this.CancelAndCloseButton.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("ca36cc8a-db09-4a0f-a022-996bcb6e34c1", "Cancel");
			this.CancelAndCloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelAndCloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(277, 3, true);
			this.CancelAndCloseButton.Name = "CancelAndCloseButton";
			this.CancelAndCloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelAndCloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CancelAndCloseButton.TabIndex = 1;
			this.CancelAndCloseButton.UseVisualStyleBackColor = true;
			this.CancelAndCloseButton.Click += new System.EventHandler(this.CancelButton_Click);
			// 
			// TypeDropEdit
			// 
			this.TypeDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.TypeDropEdit, "SN_Type");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.MasterFiles.Business.ViewStmNums)(null)).SN_Type)));
			this.TypeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 12, true);
			this.TypeDropEdit.Name = "TypeDropEdit";
			this.TypeDropEdit.PreBoundMaxLength = 3;
			this.TypeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(265, 18, true);
			this.TypeDropEdit.TabIndex = 0;
			// 
			// PrefixTextBox
			// 
			this.BindingSource.SetBindingMember(this.PrefixTextBox, "SN_Prefix");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.ViewStmNums)(null)).SN_Prefix)));
			this.PrefixTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 38, true);
			this.PrefixTextBox.Name = "PrefixTextBox";
			this.PrefixTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(219, 18, true);
			this.PrefixTextBox.TabIndex = 1;
			// 
			// ValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.ValueCalcEdit, "SN_ValueForDisplay");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(null)).SN_ValueForDisplay)));
			this.ValueCalcEdit.DecimalPlaces = 2;
			this.ValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 64, true);
			this.ValueCalcEdit.Name = "ValueCalcEdit";
			this.ValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.ValueCalcEdit.TabIndex = 4;
			this.ValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinimumValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MinimumValueCalcEdit, "SN_MinimumValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(null)).SN_MinimumValue)));
			this.MinimumValueCalcEdit.DecimalPlaces = 2;
			this.MinimumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 90, true);
			this.MinimumValueCalcEdit.Name = "MinimumValueCalcEdit";
			this.MinimumValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.MinimumValueCalcEdit.TabIndex = 5;
			this.MinimumValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// CountCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.CountCalcEdit, "SN_Count");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(null)).SN_Count)));
			this.CountCalcEdit.DecimalPlaces = 2;
			this.CountCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 116, true);
			this.CountCalcEdit.Name = "CountCalcEdit";
			this.CountCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.CountCalcEdit.TabIndex = 6;
			this.CountCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaximumValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.MaximumValueCalcEdit, "SN_MaximumValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.ViewStmNums)(null)).SN_MaximumValue)));
			this.MaximumValueCalcEdit.DecimalPlaces = 2;
			this.MaximumValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(87, 142, true);
			this.MaximumValueCalcEdit.Name = "MaximumValueCalcEdit";
			this.MaximumValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 18, true);
			this.MaximumValueCalcEdit.TabIndex = 7;
			this.MaximumValueCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ViewStmNumsEditorForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CancelButton = this.CancelAndCloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(362, 223, true);
			this.Controls.Add(this.MaximumValueCalcEdit);
			this.Controls.Add(this.CountCalcEdit);
			this.Controls.Add(this.MinimumValueCalcEdit);
			this.Controls.Add(this.ValueCalcEdit);
			this.Controls.Add(this.PrefixTextBox);
			this.Controls.Add(this.TypeDropEdit);
			this.Controls.Add(this.BottomPanel);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.ViewStmNums);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.Name = "ViewStmNumsEditorForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.BottomPanel, 0);
			this.Controls.SetChildIndex(this.TypeDropEdit, 0);
			this.Controls.SetChildIndex(this.PrefixTextBox, 0);
			this.Controls.SetChildIndex(this.ValueCalcEdit, 0);
			this.Controls.SetChildIndex(this.MinimumValueCalcEdit, 0);
			this.Controls.SetChildIndex(this.CountCalcEdit, 0);
			this.Controls.SetChildIndex(this.MaximumValueCalcEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.BottomPanel.ResumeLayout(false);
			this.BottomPanel.PerformLayout();
			this.TypeDropEdit.ResumeLayout(true);
			this.TypeDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.GUI.ZPanel BottomPanel;
		private ZArchitecture.GUI.ZDropEdit TypeDropEdit;
		public ZArchitecture.ZTextBox PrefixTextBox;
		private ZArchitecture.ZCalcEdit ValueCalcEdit;
		private ZArchitecture.ZCalcEdit MinimumValueCalcEdit;
		private ZArchitecture.ZCalcEdit MaximumValueCalcEdit;
		private ZArchitecture.GUI.ZButton ValidateAndSaveButton;
		private ZArchitecture.GUI.ZButton CancelAndCloseButton;
		private ZArchitecture.ZCalcEdit CountCalcEdit;
	}
}
