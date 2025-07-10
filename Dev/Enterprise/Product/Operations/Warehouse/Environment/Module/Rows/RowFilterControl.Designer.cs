using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Environment.Module
{
	public partial class RowFilterControl
	{
		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo3 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo4 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			this.grid.AllowDrop = true;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("RowFilterControl|WR_WW_Whs", "Warehouse");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "WR_WW_Whs";
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("RowFilterControl|WR_Name", "Row");
			zTextBoxColumnStyleInfo1.ColumnName = "WR_Name";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("RowFilterControl|WR_Columns", "Columns");
			zCalcEditColumnStyleInfo1.ColumnName = "WR_Columns";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("RowFilterControl|WR_Levels", "Levels");
			zCalcEditColumnStyleInfo2.ColumnName = "WR_Levels";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo3.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo3.CaptionResourceString = Enterprise.Warehouse.Environment.Module.Res.GetData("RowFilterControl|WR_Trays", "Trays");
			zCalcEditColumnStyleInfo3.ColumnName = "WR_Trays";
			zCalcEditColumnStyleInfo3.Decimals = 0;
			zCalcEditColumnStyleInfo4.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo4.ColumnName = "WR_PickPathSequence";
			zCalcEditColumnStyleInfo4.Decimals = 0;
			zCalcEditColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo4);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 80, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 466, true);
			this.grid.TabIndex = 8;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.WhsRow);
			// 
			// RowFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RowFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(716, 546, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
