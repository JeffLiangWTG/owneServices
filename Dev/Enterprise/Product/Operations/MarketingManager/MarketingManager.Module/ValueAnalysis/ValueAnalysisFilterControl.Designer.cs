using System.Windows.Forms;
using Enterprise.MarketingManager.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.MarketingManager.Module
{
	public partial class ValueAnalysisFilterControl
	{
		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo2 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo3 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo4 = new ZCodeFindBoxColumnStyleInfo();
			ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo5 = new ZCodeFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("ea5f0c02-f2cc-4c06-b31f-bd614be270eb", "Status");
			zDropEditColumnStyleInfo1.ColumnName = "VVA_Status";
			zDropEditColumnStyleInfo1.IsReadOnly = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("39d054f3-4342-43f2-88b4-bc47e5e10ea4", "Traded");
			zCheckBoxColumnStyleInfo1.ColumnName = "VVA_IsTraded";
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("c4da0e08-abc6-42dd-a6ac-18d3c012e00f", "Origin");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "Origin+VLO_Code";
			zCodeFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("620aad52-155d-427c-b415-3cad4fb17e72", "Destination");
			zCodeFindBoxColumnStyleInfo2.ColumnName = "Destination+VLO_Code";
			zCodeFindBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("7c30b751-89aa-4a3c-a557-22611428d6e5", "Warehouse Name");
			zTextBoxColumnStyleInfo1.ColumnName = "VVA_Warehouse";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(115);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("1c989bf7-3083-42fe-a322-54b03a8d9ca8", "Last Activity Date");
			zDateEditColumnStyleInfo1.ColumnName = "VVA_LastActivity";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("ed2f57fe-bcac-4826-81c4-5a05723f6694", "Mode");
			zTextBoxColumnStyleInfo2.ColumnName = "VVA_TradeMode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("a7a9a6fe-175e-4189-a2c2-6563ef29a32b", "Service");
			zTextBoxColumnStyleInfo3.ColumnName = "VVA_Service";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("93f5dee5-ed9d-4f4f-b8c1-31ceb86afd6e", "Type");
			zTextBoxColumnStyleInfo4.ColumnName = "VVA_TradeType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zCodeFindBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("a7e8c71f-ca86-4204-a12d-6e3f780ea346", "Buyer");
			zCodeFindBoxColumnStyleInfo3.ColumnName = "Buyer+OH_Code";
			zCodeFindBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("f9bc8475-2925-406b-8452-954008b01537", "Supplier");
			zCodeFindBoxColumnStyleInfo4.ColumnName = "Supplier+OH_Code";
			zCodeFindBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCodeFindBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("c89aa4d7-df67-4298-8708-31471b769764", "Client");
			zCodeFindBoxColumnStyleInfo5.ColumnName = "Primary+OH_Code";
			zCodeFindBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MarketingManager.Module.Res.GetData("34d19f54-3264-477f-9ecf-0c573db88dda", "Location");
			zTextBoxColumnStyleInfo5.ColumnName = "Location";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			this.grid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 264, true);
			this.grid.MouseDoubleClick += new MouseEventHandler(this.grid_MouseDoubleClick);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(ViewValueAnalysis);
			// 
			// ValueAnalysisFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ValueAnalysisFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1275, 416, true);
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
