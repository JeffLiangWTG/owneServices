using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Enterprise.Customs.US.Module
{
	partial class USCarrierCombinedFilterControl
	{
		void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("a2d78c1f-4fa0-4e96-814f-0342e1ad21af", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "UI_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("00d4398c-e425-4b06-b1b2-034f825f709b", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "UI_Name";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(250);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("c8ba5301-eb38-4eef-bfed-4511a162d4a6", "Address");
			zTextBoxColumnStyleInfo3.ColumnName = "UI_Address";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("6f6030ae-ba26-48cf-a8d4-82562939529b", "Airway Bill Prefix");
			zTextBoxColumnStyleInfo4.ColumnName = "UI_AirwayBillPrefix";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("04a61fd2-aeff-4980-a5b6-c87baf4650e3", "Mode Of Transportation");
			zTextBoxColumnStyleInfo5.ColumnName = "UI_ModeOfTransportation";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Customs.US.Module.Res.GetData("9a2f56ba-b908-4cee-ab67-035874c213ca", "Is System Generated");
			zTextBoxColumnStyleInfo6.ColumnName = "UI_IsSystem";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.USCarrierCombined);
			// 
			// USCarrierCombinedFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "USCarrierCombinedFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
