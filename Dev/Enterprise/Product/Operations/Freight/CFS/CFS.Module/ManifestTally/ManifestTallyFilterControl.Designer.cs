using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.Freight.CFS.Module
{
	public partial class ManifestTallyFilterControl
	{
		#region Component Designer generated code

		/// <summary> 
		/// Required method for Designer support - do not modify 
		/// the contents of this method with the code editor.
		/// </summary>
		private void InitializeComponent()
		{
			ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new ZGuidFindBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo3 = new ZArchitecture.ZDateEditColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zGuidFindBoxColumnStyleInfo1.Caption = null;
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|24bb7eb7-4f93-4f8f-b47a-192e8d4dc60d", "Client");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "JC_OH_CFSClient";
			zTextBoxColumnStyleInfo1.Caption = null;
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|02dcb6b4-c43d-4a0b-8803-acf334dc3a6e", "Container No.");
			zTextBoxColumnStyleInfo1.ColumnName = "JC_ContainerNum";
			zTextBoxColumnStyleInfo2.Caption = null;
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|e24ebdce-4eda-48a2-bc56-f78f5f29a8d3", "Container Mode");
			zTextBoxColumnStyleInfo2.ColumnName = "JC_ContainerMode";
			zDateEditColumnStyleInfo1.Caption = null;
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|bd06826c-100d-4c6e-9f7f-b87afe6f9c2a", "Unpack");
			zDateEditColumnStyleInfo1.ColumnName = "JC_LCLUnpack";
			zDateEditColumnStyleInfo1.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo2.Caption = null;
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|d58c505b-67ad-415f-8cad-68fba97e7e10", "Available");
			zDateEditColumnStyleInfo2.ColumnName = "JC_LCLAvailable";
			zDateEditColumnStyleInfo2.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zDateEditColumnStyleInfo3.Caption = null;
			zDateEditColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|1fcd8909-a4f0-4920-844c-acbdfb7b4c99", "Storage");
			zDateEditColumnStyleInfo3.ColumnName = "JC_LCLStorageCommences";
			zDateEditColumnStyleInfo3.DateTimeFormat = Enterprise.ZArchitecture.Core.ZDateTimePickerFormat.Short;
			zTextBoxColumnStyleInfo3.Caption = null;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|64e7597c-ef26-4058-9ee9-f3d3805982fe", "Job ID");
			zTextBoxColumnStyleInfo3.ColumnName = "JC_ContainerJobID";
			zTextBoxColumnStyleInfo4.Caption = null;
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|62157b8f-4c8c-40af-ba0b-12b3c8a3ce60", "Unpack Gang");
			zTextBoxColumnStyleInfo4.ColumnName = "JC_UnpackGang";
			zTextBoxColumnStyleInfo5.Caption = null;
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|d6ff9ec8-6f71-4eeb-9535-4e5662470577", "Port Of Loading");
			zTextBoxColumnStyleInfo5.ColumnName = "JC_JA_NKPortOfLoading";
			zTextBoxColumnStyleInfo6.Caption = null;
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|6aeb4258-95c5-41a2-9f31-c00370e2a6ab", "Port Of Discharge");
			zTextBoxColumnStyleInfo6.ColumnName = "JC_JB_NKPortOfDischarge";
			zTextBoxColumnStyleInfo7.Caption = null;
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|13b22f8a-eced-4c0e-9ad9-485abcf9ab71", "Vessel");
			zTextBoxColumnStyleInfo7.ColumnName = "JC_JV_NKVessel";
			zTextBoxColumnStyleInfo8.Caption = null;
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|89ed9e82-2be3-4dd1-8979-067487cdfa2d", "Voyage");
			zTextBoxColumnStyleInfo8.ColumnName = "JC_JV_VoyageFlight";
			zTextBoxColumnStyleInfo9.Caption = null;
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.Freight.CFS.Module.Res.GetData("ManifestTallyFilterControl|4f6c3271-7e2b-48a7-8e22-7bb6a9722c75", "Load List no");
			zTextBoxColumnStyleInfo9.ColumnName = "JC_JK_UniqueConsignRef";
			this.grid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zDateEditColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.grid.Location = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledPoint(0, 192, true);
			this.grid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 408, true);
			this.grid.TabIndex = 19;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Business.TallyContainer);
			// 
			// ManifestTallyFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "ManifestTallyFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(800, 600, true);
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}

		#endregion
	}
}
