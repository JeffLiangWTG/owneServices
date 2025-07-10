using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.Common.Module
{
	public partial class LocalCartageJobTypeFilterControl : ZFilterStripControl
	{
		void InitializeComponent()
		{
			ZDropEditColumnStyleInfo zDropEditColumnStyleInfo1 = new ZDropEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new ZArchitecture.ZCheckBoxColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zDropEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Common.Module.Res.GetData("LocalCartageJobTypeFilterControl|878ccf21-8343-42a9-be04-846e506b321e", "Job Type");
			zDropEditColumnStyleInfo1.ColumnName = "E3_JobType";
			zDropEditColumnStyleInfo1.IsMandatory = true;
			zDropEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Common.Module.Res.GetData("LocalCartageJobTypeFilterControl|a100343a-fd34-4161-b4ad-2b64790a7241", "Description");
			zTextBoxColumnStyleInfo1.ColumnName = "E3_DescriptionMultilingual";
			zTextBoxColumnStyleInfo1.IsMandatory = true;
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(200);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Common.Module.Res.GetData("LocalCartageJobTypeFilterControl|9a5b6614-4e9d-468f-a7c8-7bd2693237d4", "Transport Mode");
			zTextBoxColumnStyleInfo2.ColumnName = "E3_ShippingTransportMode";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(100);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Common.Module.Res.GetData("LocalCartageJobTypeFilterControl|9c87127e-3593-4465-bb1d-32ba7e798289", "Is Hidden");
			zCheckBoxColumnStyleInfo1.ColumnName = "E3_IsHidden";
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.Common.Module.Res.GetData("LocalCartageJobTypeFilterControl|66dd55e2-7742-4c9f-a137-2ad7c05d359c", "Is System");
			zCheckBoxColumnStyleInfo2.ColumnName = "E3_IsSystem";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.Common.Module.Res.GetData("LocalCartageJobTypeFilterControl|fed88cf9-560d-46dd-9109-07b4233e2cf5", "Department");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "E3_GE";
			this.FilteredGrid.ColumnStyles.Add(zDropEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 88, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 382, true);
			this.FilteredGrid.TabIndex = 14;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CommonCartageType);
			// 
			// LocalCartageJobTypeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "LocalCartageJobTypeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(734, 470, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
