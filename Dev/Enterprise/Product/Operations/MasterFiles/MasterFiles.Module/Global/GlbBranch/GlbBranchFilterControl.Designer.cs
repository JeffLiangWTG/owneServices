namespace Enterprise.MasterFiles.Module
{
	public partial class GlbBranchFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo7 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo8 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo9 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo10 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo11 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo12 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo13 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo14 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.ColumnName = "GB_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbBranchFilterControl|ee856d2e-9173-4da4-80ee-b9fbf76de503", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "GB_BranchName";
			zCheckBoxColumnStyleInfo1.ColumnName = "GB_IsActive";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GB_GC";
			zTextBoxColumnStyleInfo13.ColumnName = "GB_AccountingGroupCode";
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("a0193a1d-681e-41f8-8d76-3b7ce8cdc5e6", "Branch Management Code");
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zGuidFindBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbBranchFilterControl|22c3ef8d-3e79-4a2d-b27a-eb0aaa330856", "Org. Proxy", "Organization Proxy");
			zGuidFindBoxColumnStyleInfo2.ColumnName = "GB_OH_OrgProxy";
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbBranchFilterControl|bd94530c-9437-4ea7-9d11-9d4f2f1bb18f", "Location");
			zTextBoxColumnStyleInfo3.ColumnName = "GB_RL_NKHomePort";
			zTextBoxColumnStyleInfo4.ColumnName = "GB_Address1";
			zTextBoxColumnStyleInfo5.ColumnName = "GB_Address2";
			zTextBoxColumnStyleInfo6.ColumnName = "GB_City";
			zTextBoxColumnStyleInfo7.ColumnName = "GB_State";
			zTextBoxColumnStyleInfo8.ColumnName = "GB_PostCode";
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbBranchFilterControl|61ad1b73-4d2c-4098-9787-3371b20f4f2d", "Phone");
			zTextBoxColumnStyleInfo9.ColumnName = "GB_Phone";
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbBranchFilterControl|d4b45462-020a-4039-a35e-00ce47d4ea33", "Fax");
			zTextBoxColumnStyleInfo10.ColumnName = "GB_Fax";
			zTextBoxColumnStyleInfo11.ColumnName = "GB_Email";
			zTextBoxColumnStyleInfo12.ColumnName = "GB_WebAddress";
			zTextBoxColumnStyleInfo14.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbBranchFilterControl|29529BCD-5342-4A67-B2D2-DCA0F4EA6891", "Country/Region");
			zTextBoxColumnStyleInfo14.ColumnName = "BaseCountry+Code";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo14);
			this.FilteredGrid.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbBranch);
			// 
			// GlbBranchFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "GlbBranchFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
