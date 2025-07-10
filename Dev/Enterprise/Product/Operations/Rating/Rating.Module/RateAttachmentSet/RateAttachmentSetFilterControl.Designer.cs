namespace Enterprise.Rating.Module
{
	partial class RateAttachmentSetFilterControl
	{
		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|4c72a6ae-215c-4d2d-a6cf-930ede97d774", "Attachment Name");
			zTextBoxColumnStyleInfo1.ColumnName = "TS_AttachmentNameMultilingual";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo2.ColumnName = "TS_TemplateType";
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = null;
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|015171ec-bd55-43d2-aa43-7886306f4ffb", "Seq");
			zCalcEditColumnStyleInfo1.ColumnName = "TS_Sequence";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(30);
			zCheckBoxColumnStyleInfo1.ColumnName = "TS_IsSystemDefined";
			zCheckBoxColumnStyleInfo1.IsVisible = false;
			zCheckBoxColumnStyleInfo2.ColumnName = "TS_IsClientSpecific";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|a352c94b-c629-43d0-bfc9-8ed7ec49e7ba", "Cover Page");
			zCheckBoxColumnStyleInfo3.ColumnName = "TS_IsCoverPage";
			zCheckBoxColumnStyleInfo3.IsVisible = false;
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|7e1bcb78-5d5e-4ebe-81b4-487e9d448698", "Standard Pricing");
			zCheckBoxColumnStyleInfo4.ColumnName = "TS_IsStandardPricingPage";
			zCheckBoxColumnStyleInfo4.IsVisible = false;
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zCheckBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|57be1837-c488-4739-8035-1c461dad512f", "One Off Pricing");
			zCheckBoxColumnStyleInfo5.ColumnName = "TS_IsOneOffPricingPage";
			zCheckBoxColumnStyleInfo5.IsVisible = false;
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|f025957f-6067-41f9-b8d0-58efd2aca02d", "Trailing Page");
			zCheckBoxColumnStyleInfo6.ColumnName = "TS_IsTrailingPage";
			zCheckBoxColumnStyleInfo6.IsVisible = false;
			zCheckBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|a7244910-dd29-4360-a703-b24c232875bc", "Default");
			zCheckBoxColumnStyleInfo7.ColumnName = "TS_IsDefault";
			zCheckBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|d6a4c015-fa26-4d42-8519-b4a8d7a203e6", "Mandatory");
			zCheckBoxColumnStyleInfo8.ColumnName = "TS_IsMandatory";
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Rating.Module.Res.GetData("RateAttachmentSetFilterControl|26018900-0247-4241-86cc-31ae738aa10a", "Company");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "TS_GC";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 40, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(445, 154, true);
			this.FilteredGrid.TabIndex = 11;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.Rating.Business.RateAttachmentSet);
			// 
			// RateAttachmentSetFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "RateAttachmentSetFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();

		}

		private System.ComponentModel.Container components = null;
	}
}
