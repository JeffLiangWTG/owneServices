namespace Enterprise.Customs.GUI
{
	partial class AllocateWeightForm
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
		protected new void InitializeComponent()
		{
			this.SelectedLinesTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.NetWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.GrossWeightCalcDropEdit = new Enterprise.ZArchitecture.GUI.ZCalcDropEdit();
			this.MethodDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.OverrideExistingCheckBox = new Enterprise.ZArchitecture.GUI.ZCheckBox();
			this.OKButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CancelButton1 = new Enterprise.ZArchitecture.GUI.ZButton();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.NetWeightCalcDropEdit.SuspendLayout();
			this.GrossWeightCalcDropEdit.SuspendLayout();
			this.MethodDropEdit.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 201, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Business.AllocateWeight);
			// 
			// SelectedLinesTextBox
			// 
			this.BindingSource.SetBindingMember(this.SelectedLinesTextBox, "SelectedLinesNumbersFormatString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.AllocateWeight)(null)).SelectedLinesNumbersFormatString)));
			this.SelectedLinesTextBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("18a685a2-3776-46fe-a926-8859fe4c8c7b", "Selected Lines");
			this.SelectedLinesTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 21, true);
			this.SelectedLinesTextBox.Name = "SelectedLinesTextBox";
			this.SelectedLinesTextBox.ReadOnly = true;
			this.SelectedLinesTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.SelectedLinesTextBox.TabIndex = 1;
			this.SelectedLinesTextBox.TabStop = false;
			// 
			// NetWeightCalcDropEdit
			// 
			this.NetWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.NetWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.AllocateWeight)(null)).NetWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.AllocateWeight)(null)).NetWeightUnit)));
			this.NetWeightCalcDropEdit.BindToAmount = "NetWeight";
			this.NetWeightCalcDropEdit.BindToUnit = "NetWeightUnit";
			this.NetWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("3818cfab-c34a-4112-a2a8-c7b691867c4a", "Net Weight");
			this.NetWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 49, true);
			this.NetWeightCalcDropEdit.Name = "NetWeightCalcDropEdit";
			this.NetWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.NetWeightCalcDropEdit.TabIndex = 2;
			// 
			// GrossWeightCalcDropEdit
			// 
			this.GrossWeightCalcDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.GrossWeightCalcDropEdit, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.INumericZType)(((Enterprise.Customs.Business.AllocateWeight)(null)).GrossWeight)));
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Business.AllocateWeight)(null)).GrossWeightUnit)));
			this.GrossWeightCalcDropEdit.BindToAmount = "GrossWeight";
			this.GrossWeightCalcDropEdit.BindToUnit = "GrossWeightUnit";
			this.GrossWeightCalcDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("ac923ea3-7a8f-4ecb-96d3-d7c52daf9b3e", "Gross Weight");
			this.GrossWeightCalcDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 79, true);
			this.GrossWeightCalcDropEdit.Name = "GrossWeightCalcDropEdit";
			this.GrossWeightCalcDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.GrossWeightCalcDropEdit.TabIndex = 3;
			// 
			// MethodDropEdit
			// 
			this.MethodDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.MethodDropEdit, "AllocateWeightMethod");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((object)(((Enterprise.Customs.Business.AllocateWeight)(null)).AllocateWeightMethod)));
			this.MethodDropEdit.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("c00d5aaa-923a-4df1-b45d-df195601258e", "Method");
			this.MethodDropEdit.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.MethodDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(118, 109, true);
			this.MethodDropEdit.Name = "MethodDropEdit";
			this.MethodDropEdit.PreBoundMaxLength = 3;
			this.MethodDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(244, 18, true);
			this.MethodDropEdit.TabIndex = 4;
			// 
			// OverrideExistingCheckBox
			// 
			this.BindingSource.SetBindingMember(this.OverrideExistingCheckBox, "OverrideExisting");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Business.AllocateWeight)(null)).OverrideExisting)));
			this.OverrideExistingCheckBox.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("2db69334-dc7e-4490-a85e-820f90589996", "Override Existing");
			this.OverrideExistingCheckBox.CheckAlign = Enterprise.ZArchitecture.GUI.ZContentAlignment.Right;
			this.OverrideExistingCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideExistingCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(12, 135, true);
			this.OverrideExistingCheckBox.Name = "OverrideExistingCheckBox";
			this.OverrideExistingCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(118, 19, true);
			this.OverrideExistingCheckBox.TabIndex = 5;
			this.OverrideExistingCheckBox.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
			this.OverrideExistingCheckBox.UseVisualStyleBackColor = true;
			// 
			// OKButton
			// 
			this.OKButton.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("86c9f9e9-acac-474a-a3ff-240d1616e370", "Allocate Weight");
			this.OKButton.IsCaptionOverridden = false;
			this.OKButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(154, 170, true);
			this.OKButton.Name = "OKButton";
			this.OKButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OKButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(105, 23, true);
			this.OKButton.TabIndex = 6;
			this.OKButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OKButton.ToolTipCaption = null;
			this.OKButton.UseVisualStyleBackColor = true;
			this.OKButton.Click += new System.EventHandler(this.OKButton_Click);
			// 
			// CancelButton1
			// 
			this.CancelButton1.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("29e587a4-3cc6-4ffa-8bea-10e0972ddda3", "Cancel");
			this.CancelButton1.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CancelButton1.IsCaptionOverridden = false;
			this.CancelButton1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(278, 170, true);
			this.CancelButton1.Name = "CancelButton1";
			this.CancelButton1.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CancelButton1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(83, 23, true);
			this.CancelButton1.TabIndex = 7;
			this.CancelButton1.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CancelButton1.ToolTipCaption = null;
			this.CancelButton1.UseVisualStyleBackColor = true;
			// 
			// AllocateWeightForm
			// 
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.CaptionRenderingEnabled = true;
			this.CaptionResourceString = Enterprise.Customs.GUI.Res.GetData("b89cef84-d46f-4286-b806-eae269b11edd", "Allocate Weight Form");
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(399, 225, true);
			this.Controls.Add(this.CancelButton1);
			this.Controls.Add(this.OKButton);
			this.Controls.Add(this.OverrideExistingCheckBox);
			this.Controls.Add(this.MethodDropEdit);
			this.Controls.Add(this.GrossWeightCalcDropEdit);
			this.Controls.Add(this.NetWeightCalcDropEdit);
			this.Controls.Add(this.SelectedLinesTextBox);
			this.DataSourceType = typeof(Enterprise.Customs.Business.AllocateWeight);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
			this.Name = "AllocateWeightForm";
			this.Controls.SetChildIndex(this.SelectedLinesTextBox, 0);
			this.Controls.SetChildIndex(this.NetWeightCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.GrossWeightCalcDropEdit, 0);
			this.Controls.SetChildIndex(this.MethodDropEdit, 0);
			this.Controls.SetChildIndex(this.OverrideExistingCheckBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.OKButton, 0);
			this.Controls.SetChildIndex(this.CancelButton1, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.NetWeightCalcDropEdit.ResumeLayout(true);
			this.NetWeightCalcDropEdit.PerformLayout();
			this.GrossWeightCalcDropEdit.ResumeLayout(true);
			this.GrossWeightCalcDropEdit.PerformLayout();
			this.MethodDropEdit.ResumeLayout(true);
			this.MethodDropEdit.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		private ZArchitecture.ZTextBox SelectedLinesTextBox;
		private ZArchitecture.GUI.ZCalcDropEdit NetWeightCalcDropEdit;
		private ZArchitecture.GUI.ZCalcDropEdit GrossWeightCalcDropEdit;
		private ZArchitecture.GUI.ZDropEdit MethodDropEdit;
		private ZArchitecture.GUI.ZCheckBox OverrideExistingCheckBox;
		private ZArchitecture.GUI.ZButton OKButton;
		private ZArchitecture.GUI.ZButton CancelButton1;
	}
}
