using Enterprise.ZArchitecture;

namespace Enterprise.Warehouse.Transactions.GUI
{
	public abstract partial class PickableDocketLinesGridUserControl
	{
		ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo11;
		protected ZTextBoxColumnStyleInfo AllocationKeyTextBoxColumnStyleInfo;

		void InitializeComponent()
		{
			ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZCheckBoxColumnStyleInfo();
			ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).BeginInit();
			this.LinesGrid.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// LinesGrid
			// 
			zGuidFindBoxColumnStyleInfo1.BindToList = "Lookups.Locations";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Warehouse.Transactions.GUI.Res.GetData("OrderDocketLinesGridUserControl|69563B92-8845-4266-AF65-B633E843FB4E", "BOM Location", "BOM Staging Location", "");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "StagingLocationBOM+PK";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.ColumnName = "IsDangerousGood";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.IsVisible = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.ColumnName = "WE_AllocationKey";
			this.LinesGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.LinesGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			// 
			// PickableDocketLinesGridUserControl
			// 
			this.Name = "PickableDocketLinesGridUserControl";
			((System.ComponentModel.ISupportInitialize)(this.LinesGrid)).EndInit();
			this.LinesGrid.ResumeLayout(false);
			this.LinesGrid.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
