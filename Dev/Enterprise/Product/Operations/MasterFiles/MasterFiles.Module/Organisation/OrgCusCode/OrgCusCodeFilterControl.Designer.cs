namespace Enterprise.MasterFiles.Module
{
	public partial class OrgCusCodeFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo zOrganisationFindBoxColumnStyleInfo1 = new Enterprise.MasterFiles.GUI.ZOrganisationFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo zCodeFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZCodeFindBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCusCodeFilterControl|51c58e51-eeb0-4c25-9453-2b202dc4349f", "Code Type");
			zTextBoxColumnStyleInfo1.ColumnName = "OK_CodeType";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCusCodeFilterControl|af3a8cb8-cda7-44be-b52e-21e60c9dba04", "Customs Reg No");
			zTextBoxColumnStyleInfo2.ColumnName = "CustomsRegNoForDisplay";
			zOrganisationFindBoxColumnStyleInfo1.ColumnName = "OK_OH";
			zGuidFindBoxColumnStyleInfo1.ColumnName = "OK_OA_PremisesAddress";
			zCodeFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("OrgCusCodeFilterControl|2ac33c5b-250d-4307-ab8a-949f301e6c7b", "Country/Region");
			zCodeFindBoxColumnStyleInfo1.ColumnName = "OK_RN_NKCodeCountry";
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zOrganisationFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCodeFindBoxColumnStyleInfo1);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 264, true);
			this.FilteredGrid.TabIndex = 8;
			this.FilteredGrid.Navigate += new System.Windows.Forms.NavigateEventHandler(this.FilteredGrid_Navigate);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.OrgCusCode);
			// 
			// OrgCusCodeFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "OrgCusCodeFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(572, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
