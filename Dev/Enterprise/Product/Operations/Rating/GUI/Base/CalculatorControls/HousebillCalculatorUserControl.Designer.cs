namespace Enterprise.Rating.GUI
{
	public partial class HousebillCalculatorUserControl
	{
		private ZArchitecture.ZGrid RateLineItemsGrid;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.RateLineItemsGrid = new ZArchitecture.ZGrid();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).BeginInit();
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
			this.RateLineItemsGrid.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom
						| System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.RateLineItemsGrid.CaptionVisible = false;
			zDropEditColumnStyleInfo1.BindToList = "Lookups.HousebillReleaseTypes";
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HousebillCalculatorUserControl|8f988a22-c6dd-437c-b184-44dfc68ac356", "Code");
			zDropEditColumnStyleInfo1.ColumnName = "TM_Type";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.MaxDropDownItems = 10;
			zDropEditColumnStyleInfo1.ToolTip = "The Housebill Release Type";
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HousebillCalculatorUserControl|e636b287-1610-45d9-905b-5aa337fc2024", "Release Type");
			zTextBoxColumnStyleInfo1.ColumnName = "HousebillReleaseTypeDesc";
			zTextBoxColumnStyleInfo1.IsReadOnly = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("HousebillCalculatorUserControl|b51ad4df-c53b-4776-8b6f-369e314e4549", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "TM_RelevantValue";
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "85b0d14d-2941-445f-b227-87467c2c49da";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridHousebill";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 136, true);
			this.RateLineItemsGrid.TabIndex = 2;
			// 
			// HousebillCalculatorUserControl
			// 
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "HousebillCalculatorUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
