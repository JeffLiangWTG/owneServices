using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.MasterFiles.Module
{
	public partial class GlbPortDeliveryTimeFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|41c3d5d2-709e-42c7-8f6e-b8d12bb911fd", "Freight");
			zTextBoxColumnStyleInfo1.ColumnName = "G1_FreightMode";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|851ab4ba-efbc-4ab0-b6e2-3390d1150a8f", "Job");
			zTextBoxColumnStyleInfo2.ColumnName = "G1_JobMode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|dc4850a5-b048-42b7-8311-9556939f8a5a", "Disch.", "Discharge");
			zTextBoxColumnStyleInfo3.ColumnName = "G1_RL_NKDischargePort";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|b885b8a3-5e64-4439-8ede-4d929d2766c5", "Dest.", "Destination");
			zTextBoxColumnStyleInfo4.ColumnName = "G1_RL_NKDestinationPort";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|f90804ba-de95-4be8-9f00-bffc77305ffe", "Delay Until ETA");
			zCalcEditColumnStyleInfo1.ColumnName = "G1_DaysDelayFromArrivalToDeliver";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|8b07663e-0bba-4e62-9c04-3f240a5b41e8", "Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "G1_OH_ClientOverride";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo2.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbPortDeliveryTimeFilterControl|d258fb46-d467-44ca-a8bc-f911d3810df6", "Delay Until Delivery");
			zCalcEditColumnStyleInfo2.ColumnName = "G1_DaysFromDestinationArrivalToClientDelivery";
			zCalcEditColumnStyleInfo2.Decimals = 0;
			zCalcEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(110);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo2);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 22, true);
			this.grid.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbPortDeliveryTime);
			// 
			// GlbPortDeliveryTimeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "GlbPortDeliveryTimeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(865, 174, true);
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
