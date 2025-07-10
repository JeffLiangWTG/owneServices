using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class PercentageBreaksControl
	{
		private ZGrid RateLineItemsGrid;
		private ZGrid ApplyToRateLineItemsGrid;
		private CargoWise.Windows.UI.KSplitContainer splitContainer1;
		private ZArchitecture.GUI.ZCheckBox BreaksBasedOnValuesCheckBox;
		private ZArchitecture.GUI.ZCheckBox IncludeTaxCheckbox;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZCalcEditColumnStyleInfo();
			ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo2 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZTextBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			this.RateLineItemsGrid = new ZGrid();
			this.ApplyToRateLineItemsGrid = new ZGrid();
			this.splitContainer1 = new CargoWise.Windows.UI.KSplitContainer();
			this.BreaksBasedOnValuesCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.IncludeTaxCheckbox = new ZArchitecture.GUI.ZCheckBox();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
			this.splitContainer1.Panel1.SuspendLayout();
			this.splitContainer1.Panel2.SuspendLayout();
			this.splitContainer1.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// RateLineItemsGrid
			// 
			this.RateLineItemsGrid.AllowNavigation = false;
			this.RateLineItemsGrid.AllowSorting = false;
			this.BindingSource.SetBindingMember(this.RateLineItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_Type);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_Break);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_RelevantValue);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_BreakMinimum);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_FlatAmount);
			this.RateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "TM_Break";
			zCalcEditColumnStyleInfo1.Decimals = 1;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(45);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageBreaksControl|dfc95aaf-e866-4b81-8273-b656ad54d4a6", "Rate");
			zCalcEditColumnStyleInfo2.ColumnName = "TM_RelevantValue";
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(70);
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageBreaksControl|0dcef3f7-df55-4ff1-834f-9ad390eaa6b2", "Percentage (%)");
			zCalcEditColumnStyleInfo3.ColumnName = "TM_BreakMinimum";
			zCalcEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "TM_FlatAmount";
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo2.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.CopySelectedRowsAllowed = true;
			this.RateLineItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.RateLineItemsGrid.GridId = "e93415d2-ef4c-4848-b207-6421e271563a";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridCartage";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(260, 120, true);
			this.RateLineItemsGrid.TabIndex = 2;
			// 
			// ApplyToRateLineItemsGrid
			// 
			this.ApplyToRateLineItemsGrid.AllowNavigation = false;
			this.ApplyToRateLineItemsGrid.AllowSorting = false;
			this.ApplyToRateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo2.BindToList = "Lookups.PercentageApplyToList";
			zDropEditColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageBreaksControl|d762a1b0-4327-4bad-9ce2-05a26c67fe36", "Type");
			zDropEditColumnStyleInfo2.ColumnName = "TM_Text";
			zDropEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageBreaksControl|fa32f0f9-efa4-4f9e-83dc-aded7c41d729", "Apply To");
			zTextBoxColumnStyleInfo1.ColumnName = "ApplyToDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageBreaksControl|606194d1-0860-435b-9894-3a85813752bf", "Charge");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "CalculationOrderOrPercentOf";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CalculationOrderOrPercentOfFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo2);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.CopySelectedRowsAllowed = true;
			this.ApplyToRateLineItemsGrid.Dock = System.Windows.Forms.DockStyle.Fill;
			this.ApplyToRateLineItemsGrid.GridId = "400d6bec-502b-4e8b-ac3d-a02a47c9a580";
			this.ApplyToRateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplyToRateLineItemsGrid.LayoutKey = "RateLineItemsGridPercentage";
			this.ApplyToRateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.ApplyToRateLineItemsGrid.Name = "ApplyToRateLineItemsGrid";
			this.ApplyToRateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(178, 120, true);
			this.ApplyToRateLineItemsGrid.TabIndex = 29;
			// 
			// splitContainer1
			// 
			this.splitContainer1.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.splitContainer1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.splitContainer1.Name = "splitContainer1";
			// 
			// splitContainer1.Panel1
			// 
			this.splitContainer1.Panel1.Controls.Add(this.RateLineItemsGrid);
			// 
			// splitContainer1.Panel2
			// 
			this.splitContainer1.Panel2.Controls.Add(this.ApplyToRateLineItemsGrid);
			this.splitContainer1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(442, 120, true);
			this.splitContainer1.SplitterDistance = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(260);
			this.splitContainer1.TabIndex = 32;
			// 
			// BreaksBasedOnValuesCheckBox
			// 
			this.BreaksBasedOnValuesCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.BreaksBasedOnValuesCheckBox.AutoSize = true;
			this.BreaksBasedOnValuesCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageBreaksControl|8e7150aa-4ef5-443a-9caf-f9be9a98b763", "Breaks are based on Values rather than Measures");
			this.BreaksBasedOnValuesCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.BreaksBasedOnValuesCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.BreaksBasedOnValuesCheckBox.Name = "BreaksBasedOnValuesCheckBox";
			this.BreaksBasedOnValuesCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.BreaksBasedOnValuesCheckBox.TabIndex = 33;
			this.BreaksBasedOnValuesCheckBox.UseVisualStyleBackColor = true;
			// 
			// IncludeTaxCheckbox
			// 
			this.IncludeTaxCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.IncludeTaxCheckbox.AutoSize = true;
			this.IncludeTaxCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeTaxCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(270, 130, true);
			this.IncludeTaxCheckbox.Name = "IncludeTaxCheckbox";
			this.IncludeTaxCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(15, 14, true);
			this.IncludeTaxCheckbox.TabIndex = 34;
			this.IncludeTaxCheckbox.UseVisualStyleBackColor = true;
			//
			// UseAccumulatedCheckbox
			//
			this.UseAccumulatedCheckbox.Visible = false;
			//
			// HigherChargeableLowerRateCheckBox
			//
			this.HigherChargeableLowerRateCheckBox.Visible = false;
			// 
			// UseInclusiveBreaksCheckBox
			// 
			this.UseInclusiveBreaksCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(360, 130, true);
			this.UseInclusiveBreaksCheckBox.TabIndex = 35;
			//
			// BreaksPerDropDown
			//
			this.BreaksPerDropDown.Visible = false;
			// 
			// PercentageBreaksControl
			// 
			this.Controls.Add(this.IncludeTaxCheckbox);
			this.Controls.Add(this.BreaksBasedOnValuesCheckBox);
			this.Controls.Add(this.splitContainer1);
			this.Name = "PercentageBreaksControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 144, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).EndInit();
			this.splitContainer1.Panel1.ResumeLayout(false);
			this.splitContainer1.Panel2.ResumeLayout(false);
			((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
			this.splitContainer1.ResumeLayout(false);
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
