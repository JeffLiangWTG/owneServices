using Enterprise.ZArchitecture;

namespace Enterprise.Rating.GUI
{
	public partial class HighestChargeControl
	{
		private ZGrid applyToRateLineItemsGrid;
		private ZLabel calculatorDescription;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			this.applyToRateLineItemsGrid = new ZGrid();
			this.calculatorDescription = new ZLabel();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.applyToRateLineItemsGrid)).BeginInit();
			this.applyToRateLineItemsGrid.SuspendLayout();
			this.SuspendLayout();
			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Business.RateLineItemsView);
			//
			// applyToRateLineItemsGrid
			//
			this.applyToRateLineItemsGrid.AllowNavigation = false;
			this.applyToRateLineItemsGrid.AllowSorting = false;
			this.applyToRateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
			| System.Windows.Forms.AnchorStyles.Left
			| System.Windows.Forms.AnchorStyles.Right;
			this.BindingSource.SetBindingMember(this.applyToRateLineItemsGrid, ".");
			// The line(s) below are a compile-time check for a binding member. Reflection is used to determine the return type of collection indexes and may require an up to date compilation of your solution. If compilation fails below, try removing them and rebuild your solution. Then, open the form, move a control up and then down, and re-compile again.
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(null);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).TM_AC);
			CargoWise.ComponentModel.Design.CompileTimeCheckBindingMember.Check(((Business.RateLineItem)(null)).ChargeCodeDescription);
			this.applyToRateLineItemsGrid.CaptionVisible = false;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HighestChargeControl|05d00b89-31f8-45a8-a027-046c3827d2a7", "Charge");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TM_AC";
			zGuidFindBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HighestChargeControl|16f70731-10a7-4b43-ae80-760d72aaa408", "Charge Description");
			zTextBoxColumnStyleInfo1.ColumnName = "ChargeCodeDescription";
			zTextBoxColumnStyleInfo1.DefaultCollectionIndex = 0;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			this.applyToRateLineItemsGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.applyToRateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.applyToRateLineItemsGrid.GridId = "bf464db1-99b2-4727-8824-9580ed799ddc";
			this.applyToRateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.applyToRateLineItemsGrid.LayoutKey = "RateLineItemsGridPercentage";
			this.applyToRateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(11, 33, true);
			this.applyToRateLineItemsGrid.Name = "applyToRateLineItemsGrid";
			this.applyToRateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(264, 90, true);
			this.applyToRateLineItemsGrid.TabIndex = 12;
			//
			// calculatorDescription
			//
			this.calculatorDescription.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HighestChargeControl|42841c5c-178a-41e8-8eb5-6e1aa1b61a7c", "Charge Code with highest amount will be created when using the Highest Charge Calculator.");
			this.calculatorDescription.FontType = Enterprise.ZArchitecture.Core.OFontTypes.Normal | Enterprise.ZArchitecture.Core.OFontTypes.SansSerif;
			this.calculatorDescription.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 9, true);
			this.calculatorDescription.Name = "calculatorDescription";
			this.calculatorDescription.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(500, 15, true);
			this.calculatorDescription.TabIndex = 13;
			this.calculatorDescription.UseMnemonic = false;
			//
			// HighestChargeControl
			//
			this.Controls.Add(this.calculatorDescription);
			this.Controls.Add(this.applyToRateLineItemsGrid);
			this.Name = "HighestChargeControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.applyToRateLineItemsGrid)).EndInit();
			this.applyToRateLineItemsGrid.ResumeLayout(false);
			this.applyToRateLineItemsGrid.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
