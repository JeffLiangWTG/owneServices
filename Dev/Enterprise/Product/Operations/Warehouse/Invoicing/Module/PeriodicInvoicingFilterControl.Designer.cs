using CargoWiseOne.ResourceStrings;
using Enterprise.Warehouse.Integration;
using Enterprise.Warehouse.Invoicing.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Warehouse.Invoicing.Module
{
	public partial class PeriodicInvoicingFilterControl
	{
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zGuidFindBoxColumnStyleInfo1.ColumnName = "ET_OH_Client";
			zGuidFindBoxColumnStyleInfo2.ColumnName = "ET_WW";
			var filterBusinessObject = (PeriodicInvoicingFilterBusinessObject)this.FilterBusinessObject;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("PeriodicInvoicingFilterControl|586b6b7b-7eac-455a-b447-196a755abe17", filterBusinessObject.WarehouseFilterDescription);
			zGuidFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo1.ColumnName = "ET_StorageJobNumber";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Res.GetData("816c711b-6b67-4b9c-9009-c8f6b29d8114", "", "Client Name", "Client Full Name", "");
			zTextBoxColumnStyleInfo2.ColumnName = "ClientName";
			zDateEditColumnStyleInfo1.ColumnName = "ET_BillingDate";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.ColumnName = "ET_StorageFromDate";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.ColumnName = "ET_StorageToDate";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("PeriodicInvoicingFilterControl|1090fe95-6c12-4507-acb8-4a26cd6a371a", "Finalized");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsFinalised";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("PeriodicInvoicingFilterControl|ProcessingStatusDesc", "", "Processing Status", "Off Band Processing Status");
			zTextBoxColumnStyleInfo3.ColumnName = "ProcessingStatusDesc";
			zTextBoxColumnStyleInfo4.ColumnName = "JobHeader+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo4.IsVisible = false;
			zCalcEditColumnStyleInfo1.ColumnName = "JobHeader+JH_TotalProfitRevenueMargin";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 130, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 302, true);
			this.grid.TabIndex = 11;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(PeriodicInvoicing);
			// 
			// InvoicingFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "InvoicingFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(600, 432, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
