
namespace Enterprise.Customs.Universal.GUI
{
	partial class RateFormulaForm
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
		new void InitializeComponent()
		{
			this.FormulaTypeGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			this.FreeFormatRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.FreeRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PercentageOfCustomsValueRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.RatePerUnitRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton = new Enterprise.ZArchitecture.GUI.ZRadioButton();
			this.UnitDropEdit = new Enterprise.ZArchitecture.GUI.ZDropEdit();
			this.RatePerUnitCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.PercentageOfCustomsValueCalcEdit = new Enterprise.ZArchitecture.ZCalcEdit();
			this.FormulaTextBox = new Enterprise.ZArchitecture.ZTextBox();
			this.OkButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.CloseButton = new Enterprise.ZArchitecture.GUI.ZButton();
			this.FormulaGroupBox = new Enterprise.ZArchitecture.GUI.ZGroupBox();
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.UnitDropEdit.SuspendLayout();
			this.FormulaTypeGroupBox.SuspendLayout();
			this.FormulaGroupBox.SuspendLayout();
			this.SuspendLayout();
			// 
			// MainStatusBar
			// 
			this.MainStatusBar.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 360, true);
			this.MainStatusBar.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 24, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Customs.Universal.RateFormulaEditHelper);
			// 
			// FormulaTypeGroupBox
			// 
			this.FormulaTypeGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FormulaTypeGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("27E04CA1-30ED-49D4-B5E1-B028C7EFE3A0", "Formula Type");
			this.FormulaTypeGroupBox.Controls.Add(this.FreeFormatRadioButton);
			this.FormulaTypeGroupBox.Controls.Add(this.PercentageOfCustomsValueAndRatePerUnitRadioButton);
			this.FormulaTypeGroupBox.Controls.Add(this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton);
			this.FormulaTypeGroupBox.Controls.Add(this.RatePerUnitRadioButton);
			this.FormulaTypeGroupBox.Controls.Add(this.PercentageOfCustomsValueRadioButton);
			this.FormulaTypeGroupBox.Controls.Add(this.FreeRadioButton);
			this.FormulaTypeGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 8, true);
			this.FormulaTypeGroupBox.Name = "FormulaTypeGroupBox";
			this.FormulaTypeGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 144, true);
			this.FormulaTypeGroupBox.TabIndex = 0;
			this.FormulaTypeGroupBox.TabStop = false;
			// 
			// FreeRadioButton
			// 
			this.FreeRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.FreeRadioButton, "IsFree");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).IsFree)));
			this.FreeRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FreeRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 17, true);
			this.FreeRadioButton.Name = "FreeRadioButton";
			this.FreeRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 19, true);
			this.FreeRadioButton.TabIndex = 1;
			this.FreeRadioButton.UseVisualStyleBackColor = false;
			// 
			// PercentageOfCustomsValueRadioButton
			// 
			this.PercentageOfCustomsValueRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.PercentageOfCustomsValueRadioButton, "IsPercentageOfCustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).IsPercentageOfCustomsValue)));
			this.PercentageOfCustomsValueRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PercentageOfCustomsValueRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 38, true);
			this.PercentageOfCustomsValueRadioButton.Name = "PercentageOfCustomsValueRadioButton";
			this.PercentageOfCustomsValueRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 19, true);
			this.PercentageOfCustomsValueRadioButton.TabIndex = 3;
			this.PercentageOfCustomsValueRadioButton.UseVisualStyleBackColor = false;
			// 
			// RatePerUnitRadioButton
			// 
			this.RatePerUnitRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.RatePerUnitRadioButton, "IsRatePerUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).IsRatePerUnit)));
			this.RatePerUnitRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.RatePerUnitRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 59, true);
			this.RatePerUnitRadioButton.Name = "RatePerUnitRadioButton";
			this.RatePerUnitRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 19, true);
			this.RatePerUnitRadioButton.TabIndex = 4;
			this.RatePerUnitRadioButton.UseVisualStyleBackColor = false;
			// 
			// PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton
			// 
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton, "IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).IsPercentageOfCustomsValueWithAMinimumOfRatePerUnit)));
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 80, true);
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Name = "PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton";
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 19, true);
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.TabIndex = 5;
			this.PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton.UseVisualStyleBackColor = false;
			// 
			// PercentageOfCustomsValueAndRatePerUnitRadioButton
			// 
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.PercentageOfCustomsValueAndRatePerUnitRadioButton, "IsPercentageOfCustomsValueAndRatePerUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).IsPercentageOfCustomsValueAndRatePerUnit)));
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 101, true);
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.Name = "PercentageOfCustomsValueAndRatePerUnitRadioButton";
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 19, true);
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.TabIndex = 6;
			this.PercentageOfCustomsValueAndRatePerUnitRadioButton.UseVisualStyleBackColor = false;
			// 
			// FreeFormatRadioButton
			// 
			this.FreeFormatRadioButton.AutoCheck = false;
			this.BindingSource.SetBindingMember(this.FreeFormatRadioButton, "IsFreeFormat");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZBool)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).IsFreeFormat)));
			this.FreeFormatRadioButton.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.FreeFormatRadioButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(17, 122, true);
			this.FreeFormatRadioButton.Name = "FreeFormatRadioButton";
			this.FreeFormatRadioButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(369, 19, true);
			this.FreeFormatRadioButton.TabIndex = 7;
			this.FreeFormatRadioButton.UseVisualStyleBackColor = false;
			// 
			// UnitDropEdit
			// 
			this.UnitDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitDropEdit, "Unit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).Unit)));
			this.UnitDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 160, true);
			this.UnitDropEdit.Name = "UnitDropEdit";
			this.UnitDropEdit.PreBoundMaxLength = 3;
			this.UnitDropEdit.ShowDescriptionBox = false;
			this.UnitDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(100, 20, true);
			this.UnitDropEdit.TabIndex = 8;
			// 
			// RatePerUnitCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.RatePerUnitCalcEdit, "RatePerUnit");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).RatePerUnit)));
			this.RatePerUnitCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(280, 160, true);
			this.RatePerUnitCalcEdit.Name = "RatePerUnitCalcEdit";
			this.RatePerUnitCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.RatePerUnitCalcEdit.TabIndex = 9;
			// 
			// PercentageOfCustomsValueCalcEdit
			// 
			this.BindingSource.SetBindingMember(this.PercentageOfCustomsValueCalcEdit, "PercentageOfCustomsValue");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZDecimal)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).PercentageOfCustomsValue)));
			this.PercentageOfCustomsValueCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 190, true);
			this.PercentageOfCustomsValueCalcEdit.Name = "PercentageOfCustomsValueCalcEdit";
			this.PercentageOfCustomsValueCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(85, 17, true);
			this.PercentageOfCustomsValueCalcEdit.TabIndex = 10;
			// 
			// FormulaTextBox
			// 
			this.BindingSource.SetBindingMember(this.FormulaTextBox, "Formula");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((CargoWise.Types.ZString)(((Enterprise.Customs.Universal.RateFormulaEditHelper)(null)).Formula)));
			this.FormulaTextBox.CaptionResourceString = null;
			this.FormulaTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Normal;
			this.FormulaTextBox.Dock = System.Windows.Forms.DockStyle.Fill;
			this.FormulaTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(2, 12, true);
			this.FormulaTextBox.Multiline = true;
			this.FormulaTextBox.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			this.FormulaTextBox.Name = "FormulaTextBox";
			this.FormulaTextBox.ScrollBars = System.Windows.Forms.ScrollBars.Vertical;
			this.FormulaTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(385, 83, true);
			this.FormulaTextBox.TabIndex = 11;
			// 
			// OkButton
			// 
			this.OkButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.OkButton.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("AC0EAAC3-99B2-41D9-990D-A046624476DF", "OK");
			this.OkButton.IsCaptionOverridden = false;
			this.OkButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(145, 328, true);
			this.OkButton.Name = "OkButton";
			this.OkButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.OkButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.OkButton.TabIndex = 12;
			this.OkButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.OkButton.ToolTipCaption = null;
			this.OkButton.UseVisualStyleBackColor = true;
			this.OkButton.Click += new System.EventHandler(this.OkButton_Click);
			// 
			// CloseButton
			// 
			this.CloseButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
			this.CloseButton.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("08504CC6-C194-4BB7-ADCA-0776A89E8BF9", "Cancel");
			this.CloseButton.DialogResult = System.Windows.Forms.DialogResult.Cancel;
			this.CloseButton.IsCaptionOverridden = false;
			this.CloseButton.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(223, 328, true);
			this.CloseButton.Name = "CloseButton";
			this.CloseButton.ShouldSetReadOnlyWhenSettingIncludingChildren = false;
			this.CloseButton.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(75, 23, true);
			this.CloseButton.TabIndex = 13;
			this.CloseButton.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;
			this.CloseButton.ToolTipCaption = null;
			this.CloseButton.Click += new System.EventHandler(this.CloseButton_Click);
			// 
			// FormulaGroupBox
			// 
			this.FormulaGroupBox.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
			this.FormulaGroupBox.CaptionResourceString = Enterprise.Customs.Universal.GUI.Res.GetData("64353430-d254-4d11-906a-3a752a8d5716", "Formula");
			this.FormulaGroupBox.Controls.Add(this.FormulaTextBox);
			this.FormulaGroupBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(22, 223, true);
			this.FormulaGroupBox.Name = "FormulaGroupBox";
			this.FormulaGroupBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(389, 97, true);
			this.FormulaGroupBox.TabIndex = 2;
			this.FormulaGroupBox.TabStop = false;
			// 
			// RateFormulaForm
			// 
			this.AcceptButton = this.OkButton;
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.None;
			this.BackColor = System.Drawing.SystemColors.Window;
			this.CancelButton = this.CloseButton;
			this.CaptionRenderingEnabled = true;
			this.ClientSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(432, 384, true);
			this.Controls.Add(this.FormulaGroupBox);
			this.Controls.Add(this.FormulaTypeGroupBox);
			this.Controls.Add(this.UnitDropEdit);
			this.Controls.Add(this.RatePerUnitCalcEdit);
			this.Controls.Add(this.PercentageOfCustomsValueCalcEdit);
			this.Controls.Add(this.OkButton);
			this.Controls.Add(this.CloseButton);
			this.DataSourceType = typeof(Enterprise.Customs.Universal.RateFormulaEditHelper);
			this.MinimumSize = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(447, 421, true);
			this.Name = "RateFormulaForm";
			this.Controls.SetChildIndex(this.CloseButton, 0);
			this.Controls.SetChildIndex(this.OkButton, 0);
			this.Controls.SetChildIndex(this.FormulaTypeGroupBox, 0);
			this.Controls.SetChildIndex(this.MainStatusBar, 0);
			this.Controls.SetChildIndex(this.FormulaGroupBox, 0);
			((System.ComponentModel.ISupportInitialize)(this.MessageStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ErrorStatusBarPanel)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.FormulaTypeGroupBox.ResumeLayout(false);
			this.FormulaTypeGroupBox.PerformLayout();
			this.FormulaGroupBox.ResumeLayout(false);
			this.FormulaGroupBox.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		#endregion

		Enterprise.ZArchitecture.GUI.ZGroupBox FormulaTypeGroupBox;
		Enterprise.ZArchitecture.GUI.ZRadioButton FreeRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton PercentageOfCustomsValueRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton RatePerUnitRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton PercentageOfCustomsValueAndRatePerUnitRadioButton;
		Enterprise.ZArchitecture.GUI.ZRadioButton PercentageOfCustomsValueWithAMinimumOfRatePerUnitRadioButton;
		Enterprise.ZArchitecture.ZTextBox FormulaTextBox;
		Enterprise.ZArchitecture.ZCalcEdit PercentageOfCustomsValueCalcEdit;
		Enterprise.ZArchitecture.ZCalcEdit RatePerUnitCalcEdit;
		Enterprise.ZArchitecture.GUI.ZDropEdit UnitDropEdit;
		Enterprise.ZArchitecture.GUI.ZButton OkButton;
		Enterprise.ZArchitecture.GUI.ZButton CloseButton;
		private ZArchitecture.GUI.ZGroupBox FormulaGroupBox;
		private ZArchitecture.GUI.ZRadioButton FreeFormatRadioButton;
	}
}
