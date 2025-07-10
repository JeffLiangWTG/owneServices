namespace Enterprise.Rating.GUI
{
	public partial class NoteCalculatorUserControl
	{
		private ZArchitecture.ZGrid RateLineItemsGrid;
		private ZArchitecture.GUI.ZCheckBox ShowWithoutPrefixCheckBox;
		private System.ComponentModel.Container components = null;

		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			this.RateLineItemsGrid = new ZArchitecture.ZGrid();
			this.ShowWithoutPrefixCheckBox = new ZArchitecture.GUI.ZCheckBox();
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
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("NoteCalculatorUserControl|524e289e-c6bd-4fc3-abd2-f5b155204bbf", "Item Description");
			zTextBoxColumnStyleInfo1.ColumnName = "TM_Text";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("NoteCalculatorUserControl|61baad30-403a-44a9-bbfb-8ab3b630acf7", "Amount");
			zCalcEditColumnStyleInfo1.ColumnName = "TM_RelevantValue";
			this.RateLineItemsGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.RateLineItemsGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.RateLineItemsGrid.CopySelectedRowsAllowed = true;
			this.RateLineItemsGrid.GridId = "5566b4fc-1408-46ae-bb6e-5e2155d3e3ef";
			this.RateLineItemsGrid.HeaderForeColor = System.Drawing.SystemColors.ControlText;
			this.RateLineItemsGrid.LayoutKey = "RateLineItemsGridNote";
			this.RateLineItemsGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 3, true);
			this.RateLineItemsGrid.Name = "RateLineItemsGrid";
			this.RateLineItemsGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(448, 102, true);
			this.RateLineItemsGrid.TabIndex = 2;
			// 
			// ShowWithoutPrefixCheckBox
			// 
			this.ShowWithoutPrefixCheckBox.Anchor = System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left
						| System.Windows.Forms.AnchorStyles.Right;
			this.ShowWithoutPrefixCheckBox.CaptionResourceString = Enterprise.Rating.GUI.Res.GetData("22c75d2a-19ca-416e-895a-1fbd59a6225b", "Show on billing without \"RATE NOTE\" prefix");
			this.ShowWithoutPrefixCheckBox.FlatStyle = System.Windows.Forms.FlatStyle.System;
			this.ShowWithoutPrefixCheckBox.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(3, 109, true);
			this.ShowWithoutPrefixCheckBox.Name = "ShowWithoutPrefixCheckBox";
			this.ShowWithoutPrefixCheckBox.TabIndex = 3;
			this.ShowWithoutPrefixCheckBox.UseVisualStyleBackColor = true;
			this.ShowWithoutPrefixCheckBox.AutoSize = true;
			// 
			// NoteCalculatorUserControl
			// 
			this.Controls.Add(this.ShowWithoutPrefixCheckBox);
			this.Controls.Add(this.RateLineItemsGrid);
			this.Name = "NoteCalculatorUserControl";
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.RateLineItemsGrid)).EndInit();
			this.ResumeLayout(false);
		}
	}
}
