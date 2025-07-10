using CargoWiseOne.ResourceStrings;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;

namespace Enterprise.MasterFiles.Module
{
	public partial class ServiceLevelFilterControl
	{
		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoIsGateway = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoTransitHours = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoArrivalTime = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfoDeliveryDueTime = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfoDeliverOnWeekend = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();

			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			//
			// grid
			//
			zTextBoxColumnStyleInfo1.CaptionResourceString = Res.GetData("ServiceLevelFilterControl|c9b829ce-54ba-42a5-a6b9-e5a55f5f10fd", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "RS_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.ColumnName = "RS_DescriptionMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCheckBoxColumnStyleInfo1.ColumnName = "RS_IsActive";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.ColumnName = "RS_IsDoorToDoor";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfoIsGateway.ColumnName = "RS_IsGateway";
			zCheckBoxColumnStyleInfoIsGateway.IsVisible = false;
			zCheckBoxColumnStyleInfoIsGateway.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.ColumnName = "RS_IsSystem"; //SuppressCodeSmell Reason = constant name
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Res.GetData("ServiceLevelFilterControl|1650826C-E0D2-4414-B387-FC3EE02A583C", "Is System");
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo3.ColumnName = "RS_ServiceDeliveryType";
			zTextBoxColumnStyleInfo3.IsVisible = false;
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo4.ColumnName = "ServiceDeliveryTypeDescription";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(300);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.ColumnName = "RS_ServiceDeliveryPercentage";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfoTransitHours.ColumnName = "DefaultTransitTimeFormatted";
			zTextBoxColumnStyleInfoTransitHours.IsVisible = false;
			zTextBoxColumnStyleInfoTransitHours.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfoArrivalTime.ColumnName = "DefaultArrivalTime";
			zTextBoxColumnStyleInfoArrivalTime.IsVisible = false;
			zTextBoxColumnStyleInfoArrivalTime.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfoDeliveryDueTime.ColumnName = "ServiceDeliveryDueTime";
			zTextBoxColumnStyleInfoDeliveryDueTime.IsVisible = false;
			zTextBoxColumnStyleInfoDeliveryDueTime.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfoDeliverOnWeekend.ColumnName = "DeliverOnWeekend";
			zCheckBoxColumnStyleInfoDeliverOnWeekend.IsVisible = false;
			zCheckBoxColumnStyleInfoDeliverOnWeekend.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);

			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfoIsGateway);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);

			if (FreightDataRegistry.Instance.CalculateDeliveryDueDateByTransportMode.Value.IsActive)
			{
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfoTransitHours);
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfoArrivalTime);
				this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfoDeliveryDueTime);
				this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfoDeliverOnWeekend);
			}

			//
			// BindingSource
			//
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.RefServiceLevel);
			//
			// ServiceLevelFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "ServiceLevelFilterControl";
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
