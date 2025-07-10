using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Transactions.Module
{
	public partial class StocktakeFilterControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowDrop = true;
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = null;
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WS_WW_Whs";
			zGuidFindBoxColumnStyleInfo2.Caption = null;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("StocktakeFilterControl|75e1a88e-b4d8-484a-bda5-491ca83f7266", "Client");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "WS_OH_Client";
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("StocktakeFilterControl|e6d29b75-b8c9-4523-9ad0-6ca59c6c4cf6", "Stocktake #");
			zTextBoxColumnStyleInfo1.ColumnName = "WS_StocktakeNumber";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("StocktakeFilterControl|198d0015-3590-4fef-815c-2c36d7003551", "Date");
			zDateEditColumnStyleInfo1.ColumnName = "WS_StocktakeDate";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("StocktakeFilterControl|28ca1106-a20f-4b95-a788-63311b93311d", "Status");
			zTextBoxColumnStyleInfo2.CharacterCasing = System.Windows.Forms.CharacterCasing.Upper;
			zTextBoxColumnStyleInfo2.ColumnName = "StatusDesc";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Transactions.Module.Res.GetData("5e262fe7-8776-4ad6-b261-ffbf184ac4e2", "", "Client Name", "Client Full Name", "");
			zTextBoxColumnStyleInfo3.ColumnName = "Client+OH_FullName";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo4.ColumnName = "Job+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo1.ColumnName = "Job+JH_TotalProfitRevenueMargin";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 466, true);
			this.grid.TabIndex = 99;
			// 
			// ToolStripRecordsFoundLabel
			// 
			this.LabelCaptionRenderProvider.SetLabelCaptionVisible(this.ToolStripRecordsFoundLabel, false);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsStocktake);
			// 
			// StocktakeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "StocktakeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 546, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
