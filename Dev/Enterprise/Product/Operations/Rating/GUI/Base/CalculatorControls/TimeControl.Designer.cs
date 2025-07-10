namespace Enterprise.Rating.GUI
{
	public partial class TimeControl
	{
		private ZArchitecture.ZGrid RateLineItemsGrid;
		private ZArchitecture.ZLabel ExcludeLabel;
		private ZArchitecture.GUI.ZDropEdit ExcludeDropEdit;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo5 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo5 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.GUI.ZDropEditColumnStyleInfo zDropEditColumnStyleInfo6 = new ZArchitecture.GUI.ZDropEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo6 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			this.RateLineItemsGrid = new ZArchitecture.ZGrid();
			this.ExcludeLabel = new ZArchitecture.ZLabel();
			this.ExcludeDropEdit = new ZArchitecture.GUI.ZDropEditWithFixedWidth();
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
			zDropEditColumnStyleInfo5.ColumnName = "TM_Type";
			zCalcEditColumnStyleInfo5.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo5.ColumnName = "TM_Break";
			zCalcEditColumnStyleInfo5.Decimals = 1;
			zDropEditColumnStyleInfo6.BindToList = "Lookups.TimeUnits";
			zDropEditColumnStyleInfo6.ColumnName = "TM_BreakWeightVolume";
			zCalcEditColumnStyleInfo6.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo6.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("TimeControl|609e3169-b80f-448b-a5a2-4e1649b68c60", "Rate");
			zCalcEditColumnStyleInfo6.ColumnName = "TM_RelevantValue";
			zTextBoxColumnStyleInfo2.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo2.IsVisible = false;
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("CombinedControl|7cba3a1c-04d3-40bf-8aa4-fc28b2654700", "Reason");
			zCheckBoxColumnStyleInfo1.ColumnName = "TM_CallForPricing";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo5);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo5);
			this.RateLineItemsGrid.ColumnStyles.Add(zDropEditColumnStyleInfo6);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo6);
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.RateLineItemsGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.RateLineItemsGrid.GridId = "06aa2247-af9a-4a73-a410-aac71e826554";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridTime";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 0, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 103, true);
			this.RateLineItemsGrid.TabIndex = 4;
			// 
			// ExcludeLabel
			// 
			this.ExcludeLabel.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.ExcludeLabel.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("TimeControl|32830bc4-9a4c-46ba-8fa7-33083a46120f", "Exclude:");
			this.ExcludeLabel.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(8, 107, true);
			this.ExcludeLabel.Name = "ExcludeLabel";
			this.ExcludeLabel.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(48, 16, true);
			this.ExcludeLabel.TabIndex = 5;
			// 
			// ExcludeDropEdit
			// 
			this.ExcludeDropEdit.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left;
			this.ExcludeDropEdit.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(64, 106, true);
			this.ExcludeDropEdit.Name = "ExcludeDropEdit";
			this.ExcludeDropEdit.PreBoundMaxLength = 3;
			this.ExcludeDropEdit.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(240, 20, true);
			this.ExcludeDropEdit.TabIndex = 6;
			// 
			// TimeControl
			// 
			this.Controls.Add(this.ExcludeDropEdit);
			this.Controls.Add(this.ExcludeLabel);
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "TimeControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
