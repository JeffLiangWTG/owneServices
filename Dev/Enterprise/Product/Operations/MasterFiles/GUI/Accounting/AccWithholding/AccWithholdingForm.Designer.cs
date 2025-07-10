namespace Enterprise.MasterFiles.GUI
{
	public partial class AccWithholdingForm
	{
		#region Windows Form Designer generated code

		private Enterprise.Core.Forms.ZPostingButtonsUserControl ButtonsUserControl;
		private Enterprise.ZArchitecture.ZTextBox AW_CodeBoundTextEdit;
		private Enterprise.ZArchitecture.ZTextBox AW_DescriptionBoundTextEdit;
		private Enterprise.ZArchitecture.GUI.ZCheckBox IsActiveCheckBox;
		private Enterprise.ZArchitecture.ZCalcEdit AW_RateBoundCalcEdit;

		protected override void InitializeComponent()
		{
			this.AW_CodeBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.AW_DescriptionBoundTextEdit = new Enterprise.ZArchitecture.ZTextBox();
			this.ButtonsUserControl = new Enterprise.Core.Forms.ZPostingButtonsUserControl();
			this.IsActiveCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.AW_RateBoundCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 115, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 24, true);
			this.MainStatusBar.SizingGrip = false;
			// 
			// MessageStatusBarPanel
			// 
			this.MessageStatusBarPanel.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(584);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccWithholding);
			// 
			// AW_CodeBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AW_CodeBoundTextEdit, "AW_Code");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccWithholding)(null)).AW_Code)));
			this.AW_CodeBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 7, true);
			this.AW_CodeBoundTextEdit.Name = "AW_CodeBoundTextEdit";
			this.AW_CodeBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.AW_CodeBoundTextEdit.TabIndex = 0;
			// 
			// AW_DescriptionBoundTextEdit
			// 
			this.BindingSource.SetBindingMember(this.AW_DescriptionBoundTextEdit, "AW_Description");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.MasterFiles.Business.AccWithholding)(null)).AW_Description)));
			this.AW_DescriptionBoundTextEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.AW_DescriptionBoundTextEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 30, true);
			this.AW_DescriptionBoundTextEdit.Name = "AW_DescriptionBoundTextEdit";
			this.AW_DescriptionBoundTextEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(176, 20, true);
			this.AW_DescriptionBoundTextEdit.TabIndex = 2;
			// 
			// ButtonsUserControl
			// 
			this.ButtonsUserControl.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(342, 85, true);
			this.ButtonsUserControl.Name = "ButtonsUserControl";
			this.ButtonsUserControl.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 24, true);
			this.ButtonsUserControl.TabIndex = 4;
			// 
			// IsActiveCheckBox
			// 
			this.IsActiveCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.IsActiveCheckBox, "AW_IsActive");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.MasterFiles.Business.AccWithholding)(null)).AW_IsActive)));
			this.IsActiveCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IsActiveCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(192, 7, true);
			this.IsActiveCheckBox.Name = "IsActiveCheckBox";
			this.IsActiveCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(68, 17, true);
			this.IsActiveCheckBox.TabIndex = 1;
			// 
			// AW_RateBoundCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.AW_RateBoundCalcEdit, "AW_Rate");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.MasterFiles.Business.AccWithholding)(null)).AW_Rate)));
			this.AW_RateBoundCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(80, 52, true);
			this.AW_RateBoundCalcEdit.Name = "AW_RateBoundCalcEdit";
			this.AW_RateBoundCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 20, true);
			this.AW_RateBoundCalcEdit.TabIndex = 3;
			this.AW_RateBoundCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// AccWithholdingForm
			// 
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(584, 139, true);
			this.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("AccWithholdingForm|3b19dc3b-3e1e-47a5-a2b4-dbe683ba47d3", "WHD Tax ID");
			this.Controls.Add(this.AW_RateBoundCalcEdit);
			this.Controls.Add(this.IsActiveCheckBox);
			this.Controls.Add(this.ButtonsUserControl);
			this.Controls.Add(this.AW_DescriptionBoundTextEdit);
			this.Controls.Add(this.AW_CodeBoundTextEdit);
			this.DataSourceType = typeof(Enterprise.MasterFiles.Business.AccWithholding);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AccWithholdingForm";
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.AW_CodeBoundTextEdit, 0);
			this.Controls.SetChildIndex(this.AW_DescriptionBoundTextEdit, 0);
			this.Controls.SetChildIndex(this.ButtonsUserControl, 0);
			this.Controls.SetChildIndex(this.IsActiveCheckBox, 0);
			this.Controls.SetChildIndex(this.AW_RateBoundCalcEdit, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
		#endregion

	}
}
