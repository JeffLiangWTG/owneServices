namespace Enterprise.MasterFiles.GUI
{
	public partial class SimilarOrgMatchesModuleButtonGrid
	{

		#region Component Designer generated code

		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.SimilarOrgMatchForApprovalCollection);
			// 
			// SimilarOrgMatchesModuleButtonGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|665a0daa-8e35-4496-aac4-e3375e2115a7", "Full Name");
			zTextBoxColumnStyleInfo1.ColumnName = "OrgPatternMatch+OH_FullName";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(155);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|9d33a800-df3c-4bf7-a89c-108e709a3d16", "Street");
			zTextBoxColumnStyleInfo2.ColumnName = "OrgPatternMatch+OH_Calc_Address1";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(192);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|1dc3edc0-49f3-4d8b-aef6-1d6acfc0db7a", "Street 2");
			zTextBoxColumnStyleInfo3.ColumnName = "OrgPatternMatch+OH_Calc_Address2";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(191);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|5dad3c40-87b2-4aad-897d-7c05fd33cf03", "City");
			zTextBoxColumnStyleInfo4.ColumnName = "OrgPatternMatch+OH_Calc_City";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(87);
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|42966c51-7e2d-4fea-92e8-eab48b62e7ac", "State");
			zTextBoxColumnStyleInfo5.ColumnName = "OrgPatternMatch+OH_Calc_State";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|57270723-e275-452a-a4e4-365e71b6b11a", "UNLOCO");
			zTextBoxColumnStyleInfo6.ColumnName = "ClosestPortCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(44);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|a2eecab3-3758-47f6-b563-29dc5bc10ef1", "P. Code");
			zTextBoxColumnStyleInfo7.ColumnName = "OrgPatternMatch+OH_Calc_PostCode";
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(55);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|db4f01c5-4792-4f7b-ba82-ece78b7d5ffe", "Phone");
			zTextBoxColumnStyleInfo8.ColumnName = "OrgPatternMatch+OS_Phone";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(79);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|53b066de-1846-47b4-b6c5-f66446bd167c", "Owner Code");
			zTextBoxColumnStyleInfo9.ColumnName = "OwnerCode";
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(72);
			zCalcEditColumnStyleInfo1.BindToDecimalPlaces = "";
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.GUI.Res.GetData("SimilarOrgMatchesModuleButtonGrid|924249bb-82c5-4308-9d69-1897078a457c", "Rank");
			zCalcEditColumnStyleInfo1.ColumnName = "OrgPatternMatch+OS_Rank";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsMandatory = true;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.Name = "SimilarOrgMatchesModuleButtonGrid";
			this.ShowAttachButton = false;
			this.ShowDetachButton = false;
			this.ShowNewButton = false;
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(1128, 464, true);
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
		}

		#endregion

	}
}
