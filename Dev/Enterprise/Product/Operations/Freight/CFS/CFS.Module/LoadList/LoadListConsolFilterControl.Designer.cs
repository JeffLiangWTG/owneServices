using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	public partial class LoadListConsolFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo4 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo15 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|37ac7bf1-94c6-4443-88ae-0c69ed0122df", "Load List Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JK_UniqueConsignRef";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|048f51ac-dd5b-4896-abbb-9283ad3e7569", "Client Reference");
			zTextBoxColumnStyleInfo2.ColumnName = "JK_AgentsReference";
			zTextBoxColumnStyleInfo3.ColumnName = "JK_TransportMode";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|c191b074-bc9c-48ec-88e0-af99930d775d", "Port Of Loading");
			zTextBoxColumnStyleInfo4.ColumnName = "JK_JX_JA_RL_NKPortOfLoading";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|95d00033-2f0b-46d2-a3ec-1bda8383f81a", "Port Of Discharge");
			zTextBoxColumnStyleInfo5.ColumnName = "JK_JX_JB_RL_NKPortOfDischarge";
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|3323294d-9cc3-4d07-b26e-12667ed0269f", "ETD");
			zDateEditColumnStyleInfo1.ColumnName = "JK_JX_JA_E_DEP";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|cac7baa3-f912-49e5-a6fd-98dffe91efc2", "ETA");
			zDateEditColumnStyleInfo2.ColumnName = "JK_JX_JB_E_ARV";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|9cbbce88-3e5f-4e95-8589-8466b1e8bdff", "Vessel");
			zTextBoxColumnStyleInfo6.ColumnName = "JK_JX_JV_NKVessel";
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|d7c8477c-143e-4f7f-9758-e9d4bed5691e", "Voyage");
			zTextBoxColumnStyleInfo7.ColumnName = "JK_JX_JV_VoyageFlight";
			zTextBoxColumnStyleInfo8.ColumnName = "JK_BookingReference";
			zTextBoxColumnStyleInfo9.ColumnName = "JK_ConsolMode";
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|9af0d26b-c024-4a06-93e4-8e27b20aa9b1", "Receival Commences");
			zDateEditColumnStyleInfo3.ColumnName = "JK_DepotReceivalCommences";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|aa62a214-24d5-4377-a5f6-fdbf867e1de8", "Cut Off");
			zDateEditColumnStyleInfo4.ColumnName = "JK_DepotCutOff";
			zDateEditColumnStyleInfo4.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("LoadListConsolFilterControl|a0962399-4000-43a6-afe6-f4f58ff5d01b", "Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JK_OH_Forwarder";
			zTextBoxColumnStyleInfo10.ColumnName = "JK_CustomsReference";
			zTextBoxColumnStyleInfo11.ColumnName = "Job+JH_Status";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo14.ColumnName = "Job+JH_HoldReason";
			zTextBoxColumnStyleInfo14.IsVisible = false;
			zTextBoxColumnStyleInfo15.ColumnName = "Job+JH_ProfitLossReasonCode";
			zTextBoxColumnStyleInfo15.IsVisible = false;
			zCalcEditColumnStyleInfo1.ColumnName = "Job+JH_TotalProfitRevenueMargin";
			zCalcEditColumnStyleInfo1.IsVisible = false;
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo15);
			this.FilteredGrid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.FilteredGrid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 184, true);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 408, true);
			this.FilteredGrid.TabIndex = 21;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.CFSLoadListConsol);
			// 
			// LoadListConsolFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "LoadListConsolFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 592, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
