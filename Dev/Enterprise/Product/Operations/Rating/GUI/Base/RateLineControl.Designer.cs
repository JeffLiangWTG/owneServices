using Enterprise.Rating.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Rating.GUI
{
	public partial class RateLineControl
	{
		private ZArchitecture.ZTextBox RoundingFactorEdit;
		private CalculatorPanel CalculatorPanel;
		private ZPanel Panel;
		private ZGuidFindBox ChargeCodeFindBox;
		private ZArchitecture.ZTextBox DescriptionTextBox;
		private ZCodeFindBox CurrencyCodeFindBox;
		private ZDropEdit UnitsDropEdit;
		private ZDropEdit MultipleDropEdit;
		private ZCheckBox UseActualCheckBox;
		private ZDropEdit UnitFactorDropEdit;
		private ZDropEdit RoundingDropEdit;
		private ZArchitecture.ZCalcEdit ActualPercentageEdit;
		protected ZCheckBox OverrideDescCheckBox;
		private ZDropEdit conditionDropEdit;
		private ZArchitecture.ZTextBox expressionTextBox;
		private ZDropEdit containerOwnershipDropBox;

		private void InitializeComponent()
		{
			this.ChargeCodeFindBox = new ZGuidFindBox();
			this.DescriptionTextBox = new ZArchitecture.ZTextBox();
			this.CurrencyCodeFindBox = new ZCodeFindBox();
			this.UnitsDropEdit = new ZDropEdit();
			this.MultipleDropEdit = new ZDropEdit();
			this.UseActualCheckBox = new ZCheckBox();
			this.UnitFactorDropEdit = new ZDropEdit();
			this.RoundingDropEdit = new ZDropEdit();
			this.Panel = new ZPanel();
			this.containerOwnershipDropBox = new ZDropEdit();
			this.expressionTextBox = new ZArchitecture.ZTextBox();
			this.conditionDropEdit = new ZDropEdit();
			this.OverrideDescCheckBox = new ZCheckBox();
			this.ActualPercentageEdit = new ZArchitecture.ZCalcEdit();
			this.RoundingFactorEdit = new ZArchitecture.ZTextBox();
			this.CalculatorPanel = new CalculatorPanel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.Panel.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(RateLine);
			// 
			// ChargeCodeFindBox
			// 
			this.ChargeCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.ChargeCodeFindBox, "TL_AC");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_AC);
			this.ChargeCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 8, true);
			this.ChargeCodeFindBox.Name = "ChargeCodeFindBox";
			this.ChargeCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 21, true);
			this.ChargeCodeFindBox.TabIndex = 1;
			// 
			// DescriptionTextBox
			// 
			this.BindingSource.SetBindingMember(this.DescriptionTextBox, "TL_RateDesc");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_RateDesc);
			this.DescriptionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 32, true);
			this.DescriptionTextBox.Name = "DescriptionTextBox";
			this.DescriptionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(266, 20, true);
			this.DescriptionTextBox.TabIndex = 3;
			// 
			// CurrencyCodeFindBox
			// 
			this.CurrencyCodeFindBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.CurrencyCodeFindBox, "TL_RX_NKCurrency");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_RX_NKCurrency);
			this.CurrencyCodeFindBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 56, true);
			this.CurrencyCodeFindBox.Name = "CurrencyCodeFindBox";
			this.CurrencyCodeFindBox.PreBoundMaxLength = 3;
			this.CurrencyCodeFindBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 21, true);
			this.CurrencyCodeFindBox.TabIndex = 5;
			// 
			// UnitsDropEdit
			// 
			this.UnitsDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitsDropEdit, "TL_WeightVolume");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_WeightVolume);
			this.UnitsDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 80, true);
			this.UnitsDropEdit.Name = "UnitsDropEdit";
			this.UnitsDropEdit.PreBoundMaxLength = 2;
			this.UnitsDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(189, 20, true);
			this.UnitsDropEdit.TabIndex = 7;
			// 
			// MultipleDropEdit
			// 
			this.MultipleDropEdit.AllowDrop = true;
			this.MultipleDropEdit.Anchor = (System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right);
			this.BindingSource.SetBindingMember(this.MultipleDropEdit, "UnitMultipleAsString");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).UnitMultipleAsString);
			this.MultipleDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 80, true);
			this.MultipleDropEdit.Name = "MultipleDropEdit";
			this.MultipleDropEdit.PreBoundMaxLength = 10;
			this.MultipleDropEdit.ShowDescriptionBox = false;
			this.MultipleDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.MultipleDropEdit.TabIndex = 9;
			// 
			// UseActualCheckBox
			// 
			this.UseActualCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.UseActualCheckBox, "UseOnlyActualWeightMeasure");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).UseOnlyActualWeightMeasure);
			this.UseActualCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.UseActualCheckBox, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.UseActualCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(380, 106, true);
			this.UseActualCheckBox.Name = "UseActualCheckBox";
			this.UseActualCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 17, true);
			this.UseActualCheckBox.TabIndex = 10;
			// 
			// UnitFactorDropEdit
			// 
			this.UnitFactorDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.UnitFactorDropEdit, "TL_UnitFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_UnitFactor);
			this.UnitFactorDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 128, true);
			this.UnitFactorDropEdit.Name = "UnitFactorDropEdit";
			this.UnitFactorDropEdit.PreBoundMaxLength = 3;
			this.UnitFactorDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.UnitFactorDropEdit.TabIndex = 11;
			// 
			// RoundingDropEdit
			// 
			this.RoundingDropEdit.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.RoundingDropEdit, "TL_Rounding");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_Rounding);
			this.RoundingDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 152, true);
			this.RoundingDropEdit.Name = "RoundingDropEdit";
			this.RoundingDropEdit.PreBoundMaxLength = 3;
			this.RoundingDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(368, 20, true);
			this.RoundingDropEdit.TabIndex = 12;
			// 
			// Panel
			// 
			this.Panel.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.Panel.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;
			this.Panel.Controls.Add(this.containerOwnershipDropBox);
			this.Panel.Controls.Add(this.expressionTextBox);
			this.Panel.Controls.Add(this.conditionDropEdit);
			this.Panel.Controls.Add(this.OverrideDescCheckBox);
			this.Panel.Controls.Add(this.ActualPercentageEdit);
			this.Panel.Controls.Add(this.RoundingFactorEdit);
			this.Panel.Controls.Add(this.CalculatorPanel);
			this.Panel.Controls.Add(this.UnitsDropEdit);
			this.Panel.Controls.Add(this.UnitFactorDropEdit);
			this.Panel.Controls.Add(this.RoundingDropEdit);
			this.Panel.Controls.Add(this.UseActualCheckBox);
			this.Panel.Controls.Add(this.MultipleDropEdit);
			this.Panel.Controls.Add(this.ChargeCodeFindBox);
			this.Panel.Controls.Add(this.DescriptionTextBox);
			this.Panel.Controls.Add(this.CurrencyCodeFindBox);
			this.Panel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 8, true);
			this.Panel.Name = "Panel";
			this.Panel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(504, 414, true);
			this.Panel.TabIndex = 13;
			// 
			// containerOwnershipDropBox
			// 
			this.containerOwnershipDropBox.AllowDrop = true;
			this.BindingSource.SetBindingMember(this.containerOwnershipDropBox, "TL_ContainerOwnership");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_ContainerOwnership);
			this.containerOwnershipDropBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 176, true);
			this.containerOwnershipDropBox.Name = "containerOwnershipDropBox";
			this.containerOwnershipDropBox.PreBoundMaxLength = 10;
			this.containerOwnershipDropBox.ShowDescriptionBox = false;
			this.containerOwnershipDropBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.containerOwnershipDropBox.TabIndex = 21;
			// 
			// expressionTextBox
			// 
			this.BindingSource.SetBindingMember(this.expressionTextBox, "TL_ConditionalExpression");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_ConditionalExpression);
			this.expressionTextBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(242, 200, true);
			this.expressionTextBox.Name = "expressionTextBox";
			this.expressionTextBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.expressionTextBox.TabIndex = 20;
			// 
			// conditionDropEdit
			// 
			this.conditionDropEdit.AllowDrop = true;
			this.conditionDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.conditionDropEdit, "TL_Condition");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_Condition);
			this.conditionDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 200, true);
			this.conditionDropEdit.Name = "conditionDropEdit";
			this.conditionDropEdit.PreBoundMaxLength = 10;
			this.conditionDropEdit.ShowDescriptionBox = false;
			this.conditionDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(102, 20, true);
			this.conditionDropEdit.TabIndex = 19;
			// 
			// OverrideDescCheckBox
			// 
			this.OverrideDescCheckBox.AutoSize = true;
			this.BindingSource.SetBindingMember(this.OverrideDescCheckBox, "OverrideChargeDescription");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).OverrideChargeDescription);
			this.OverrideDescCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.OverrideDescCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(386, 34, true);
			this.OverrideDescCheckBox.Name = "OverrideDescCheckBox";
			this.OverrideDescCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(66, 17, true);
			this.OverrideDescCheckBox.TabIndex = 18;
			this.OverrideDescCheckBox.UseVisualStyleBackColor = true;
			this.OverrideDescCheckBox.CheckedChanged += new System.EventHandler(this.OverrideDescCheckBox_CheckedChanged);
			// 
			// ActualPercentageEdit
			// 
			this.BindingSource.SetBindingMember(this.ActualPercentageEdit, "TL_ActualPercentage");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).TL_ActualPercentage);
			this.ActualPercentageEdit.DecimalPlaces = 0;
			this.ActualPercentageEdit.Decimals = 0;
			this.LabelCaptionRenderProvider.SetLabelCaptionAlignment(this.ActualPercentageEdit, CargoWise.Windows.UI.LabelCaptionAlignment.Auto);
			this.ActualPercentageEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(324, 104, true);
			this.ActualPercentageEdit.Name = "ActualPercentageEdit";
			this.ActualPercentageEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(50, 20, true);
			this.ActualPercentageEdit.TabIndex = 17;
			this.ActualPercentageEdit.Text = "0";
			this.ActualPercentageEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RoundingFactorEdit
			// 
			this.BindingSource.SetBindingMember(this.RoundingFactorEdit, "RoundingFactor");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLine)(null)).RoundingFactor);
			this.RoundingFactorEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(114, 104, true);
			this.RoundingFactorEdit.Name = "RoundingFactorEdit";
			this.RoundingFactorEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(72, 20, true);
			this.RoundingFactorEdit.TabIndex = 14;
			// 
			// CalculatorPanel
			// 
			this.CalculatorPanel.AgentRatesCheckBoxVisible = true;
			this.CalculatorPanel.AllowDrop = true;
			this.CalculatorPanel.BackColor = System.Drawing.SystemColors.Control;
			this.CalculatorPanel.BindingMember = "ViewCalculator";
			this.CalculatorPanel.CalculatorDropEditVisible = true;
			this.CalculatorPanel.ForeColor = System.Drawing.SystemColors.ControlText;
			this.CalculatorPanel.IsBreakWeightVolumeAvailable = true;
			this.CalculatorPanel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 224, true);
			this.CalculatorPanel.Name = "CalculatorPanel";
			this.CalculatorPanel.RemoveAction = Enterprise.ZArchitecture.RemoveAction.RemoveAndDelete;
			this.CalculatorPanel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(494, 210, true);
			this.CalculatorPanel.TabIndex = 1;
			this.CalculatorPanel.ViewResultsVisible = false;
			// 
			// RateLineControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Controls.Add(this.Panel);
			this.Name = "RateLineControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(520, 409, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.Panel.ResumeLayout(false);
			this.Panel.PerformLayout();
			this.ResumeLayout(false);
		}
	}
}
