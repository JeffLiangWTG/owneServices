using System;
using Enterprise.Rating.Business;
using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class PercentageControl
	{
		private ZCalcEdit PercentCalcEdit;
		private ZCalcEdit MinimumCalcEdit;
		private ZCalcEdit BasePriceCalcEdit;
		private ZGrid ApplyToRateLineItemsGrid;
		private ZCalcEdit MaximumCalcEdit;
		internal ZArchitecture.GUI.ZCheckBox IncludeTaxCheckbox;
		internal ZArchitecture.GUI.ZCheckBox GreaterChargeCheckbox;
		private ZLabel PercentLabel;
		internal ZArchitecture.GUI.ZCheckBox PartThereofCheckBox;
		private ZCalcEdit RateCalcEdit;
		private ZLabel RateLabel1;
		private ZCalcEdit ValueOrPartThereOfCalcEdit;
		private ZLabel RateLabel2;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			ZArchitecture.GUI.ZMultiControlColumnStyleInfo zMultiControlColumnStyleInfo1 = new ZArchitecture.GUI.ZMultiControlColumnStyleInfo();
			this.PercentLabel = new ZLabel();
			this.PercentCalcEdit = new ZCalcEdit();
			this.MinimumCalcEdit = new ZCalcEdit();
			this.BasePriceCalcEdit = new ZCalcEdit();
			this.MaximumCalcEdit = new ZCalcEdit();
			this.ApplyToRateLineItemsGrid = new ZGrid();
			this.IncludeTaxCheckbox = new ZArchitecture.GUI.ZCheckBox();
			this.GreaterChargeCheckbox = new ZArchitecture.GUI.ZCheckBox();
			this.PartThereofCheckBox = new ZArchitecture.GUI.ZCheckBox();
			this.RateCalcEdit = new ZCalcEdit();
			this.RateLabel1 = new ZLabel();
			this.ValueOrPartThereOfCalcEdit = new ZCalcEdit();
			this.RateLabel2 = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).BeginInit();
			this.ApplyToRateLineItemsGrid.SuspendLayout();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = DataSourceTypeForBinding;
			// 
			// PercentLabel
			// 
			this.PercentLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|c8fa8130-f53b-4187-b79f-50924b5ec476", "Percentage (%):");
			this.PercentLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 0, true);
			this.PercentLabel.Name = "PercentLabel";
			this.PercentLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(88, 23, true);
			this.PercentLabel.TabIndex = 4;
			// 
			// PercentCalcEdit
			// 
			this.PercentCalcEdit.DecimalPlaces = 2;
			this.PercentCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 0, true);
			this.PercentCalcEdit.Name = "PercentCalcEdit";
			this.PercentCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.PercentCalcEdit.TabIndex = 5;
			this.PercentCalcEdit.Text = "0.0000";
			this.PercentCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MinimumCalcEdit
			// 
			this.MinimumCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|f7c82582-20f5-4901-893f-5a4728d00626", "Minimum");
			this.MinimumCalcEdit.DecimalPlaces = 2;
			this.MinimumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 0, true);
			this.MinimumCalcEdit.Name = "MinimumCalcEdit";
			this.MinimumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.MinimumCalcEdit.TabIndex = 0;
			this.MinimumCalcEdit.Text = "0.000";
			this.MinimumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// BasePriceCalcEdit
			// 
			this.BasePriceCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|fa9164bc-1144-4e79-b379-14e1ac99ff95", "Base Price");
			this.BasePriceCalcEdit.DecimalPlaces = 2;
			this.BasePriceCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 24, true);
			this.BasePriceCalcEdit.Name = "BasePriceCalcEdit";
			this.BasePriceCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.BasePriceCalcEdit.TabIndex = 1;
			this.BasePriceCalcEdit.Text = "0.000";
			this.BasePriceCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// MaximumCalcEdit
			// 
			this.MaximumCalcEdit.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|e8f16646-394e-449d-8087-fc504f13ffde", "Maximum");
			this.MaximumCalcEdit.DecimalPlaces = 2;
			this.MaximumCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(96, 48, true);
			this.MaximumCalcEdit.Name = "MaximumCalcEdit";
			this.MaximumCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.MaximumCalcEdit.TabIndex = 2;
			this.MaximumCalcEdit.Text = "0.000";
			this.MaximumCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// ApplyToRateLineItemsGrid
			// 
			this.ApplyToRateLineItemsGrid.AllowNavigation = false;
			this.ApplyToRateLineItemsGrid.AllowSorting = false;
			this.ApplyToRateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.ApplyToRateLineItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLineItem)(null)).TM_Text);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLineItem)(null)).Lookups.PercentageApplyToList);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLineItem)(null)).ApplyToDescription);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLineItem)(null)).CalculationOrderOrPercentOf);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((RateLineItem)(null)).CalculationOrderOrPercentOfFieldType);
			this.ApplyToRateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.PercentageApplyToList";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|343d5f40-298a-4b5c-b78f-967180b95eda", "Type");
			zDropEditColumnStyleInfo1.ColumnName = "TM_Text";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(35);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|f3e82947-928b-4cfc-a163-584cf56012c8", "Apply To");
			zTextBoxColumnStyleInfo1.ColumnName = "ApplyToDescription";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zMultiControlColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|7771511b-60f5-4fa4-b986-a9ad487068d6", "Chrg.");
			zMultiControlColumnStyleInfo1.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zMultiControlColumnStyleInfo1.ColumnName = "CalculationOrderOrPercentOf";
			zMultiControlColumnStyleInfo1.FieldTypeColumnName = "CalculationOrderOrPercentOfFieldType";
			zMultiControlColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.ColumnStyles.Add(zMultiControlColumnStyleInfo1);
			this.ApplyToRateLineItemsGrid.CopySelectedRowsAllowed = true;
			this.ApplyToRateLineItemsGrid.GridId = "bf464db1-99b2-4727-8824-9580ed799ddc";
			this.ApplyToRateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.ApplyToRateLineItemsGrid.LayoutKey = "RateLineItemsGridPercentage";
			this.ApplyToRateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 24, true);
			this.ApplyToRateLineItemsGrid.Name = "ApplyToRateLineItemsGrid";
			this.ApplyToRateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 90, true);
			this.ApplyToRateLineItemsGrid.TabIndex = 12;
			// 
			// IncludeTaxCheckbox
			// 
			this.IncludeTaxCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.IncludeTaxCheckbox.AutoSize = true;
			this.IncludeTaxCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.IncludeTaxCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 124, true);
			this.IncludeTaxCheckbox.Name = "IncludeTaxCheckbox";
			this.IncludeTaxCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.IncludeTaxCheckbox.TabIndex = 5;
			// 
			// GreaterChargeCheckbox
			// 
			this.GreaterChargeCheckbox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.GreaterChargeCheckbox.AutoSize = true;
			this.GreaterChargeCheckbox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|6d6dd971-99eb-49c0-a6c3-5a9f7e1bb296", "Take Highest Charge");
			this.GreaterChargeCheckbox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.GreaterChargeCheckbox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(120, 124, true);
			this.GreaterChargeCheckbox.Name = "GreaterChargeCheckbox";
			this.GreaterChargeCheckbox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.GreaterChargeCheckbox.TabIndex = 3;
			// 
			// PartThereofCheckBox
			// 
			this.PartThereofCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.PartThereofCheckBox.AutoSize = true;
			this.PartThereofCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|8c7681d9-801a-4065-acf7-de601b9e3970", "Part Thereof");
			this.PartThereofCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.PartThereofCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 124, true);
			this.PartThereofCheckBox.Name = "PartThereofCheckBox";
			this.PartThereofCheckBox.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(14, 14, true);
			this.PartThereofCheckBox.TabIndex = 4;
			this.PartThereofCheckBox.CheckedChanged += new EventHandler(this.PartThereofCheckBox_CheckedChanged);
			// 
			// RateCalcEdit
			// 
			this.RateCalcEdit.DecimalPlaces = 2;
			this.RateCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(176, 0, true);
			this.RateCalcEdit.Name = "RateCalcEdit";
			this.RateCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.RateCalcEdit.TabIndex = 6;
			this.RateCalcEdit.Text = "0.00";
			this.RateCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateLabel1
			// 
			this.RateLabel1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|9d9c67c9-d4a5-498a-84e6-b5c790e661f0", "per");
			this.RateLabel1.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(248, 0, true);
			this.RateLabel1.Name = "RateLabel1";
			this.RateLabel1.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(24, 16, true);
			this.RateLabel1.TabIndex = 18;
			// 
			// ValueOrPartThereOfCalcEdit
			// 
			this.ValueOrPartThereOfCalcEdit.DecimalPlaces = 2;
			this.ValueOrPartThereOfCalcEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(272, 0, true);
			this.ValueOrPartThereOfCalcEdit.Name = "ValueOrPartThereOfCalcEdit";
			this.ValueOrPartThereOfCalcEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(64, 18, true);
			this.ValueOrPartThereOfCalcEdit.TabIndex = 7;
			this.ValueOrPartThereOfCalcEdit.TextAlign = System.Windows.Forms.HorizontalAlignment.Right;
			// 
			// RateLabel2
			// 
			this.RateLabel2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("PercentageControl|a445ba67-ee5a-47f2-99d4-7f74c6fa1950", "or Part Thereof");
			this.RateLabel2.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(344, 0, true);
			this.RateLabel2.Name = "RateLabel2";
			this.RateLabel2.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(80, 16, true);
			this.RateLabel2.TabIndex = 18;
			// 
			// PercentageControl
			// 
			this.Controls.Add(this.ValueOrPartThereOfCalcEdit);
			this.Controls.Add(this.RateLabel1);
			this.Controls.Add(this.RateCalcEdit);
			this.Controls.Add(this.PartThereofCheckBox);
			this.Controls.Add(this.GreaterChargeCheckbox);
			this.Controls.Add(this.IncludeTaxCheckbox);
			this.Controls.Add(this.ApplyToRateLineItemsGrid);
			this.Controls.Add(this.MaximumCalcEdit);
			this.Controls.Add(this.BasePriceCalcEdit);
			this.Controls.Add(this.MinimumCalcEdit);
			this.Controls.Add(this.PercentCalcEdit);
			this.Controls.Add(this.PercentLabel);
			this.Controls.Add(this.RateLabel2);
			this.Name = "PercentageControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.ApplyToRateLineItemsGrid)).EndInit();
			this.ApplyToRateLineItemsGrid.ResumeLayout(false);
			this.ApplyToRateLineItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
