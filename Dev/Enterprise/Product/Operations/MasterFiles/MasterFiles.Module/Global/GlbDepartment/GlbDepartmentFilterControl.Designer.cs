namespace Enterprise.MasterFiles.Module
{
	public partial class GlbDepartmentFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo5 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|01713f28-882f-47f2-bff3-5e319a9fec9b", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "GE_Code";
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|ed3b766d-4c16-422a-9f4a-08e7a654f465", "Name");
			zTextBoxColumnStyleInfo2.ColumnName = "GE_DescMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(150);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|648296af-d9e8-4889-a445-645d2a204289", "Parent");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "GE_GE";
			zGuidFindBoxColumnStyleInfo1.IsVisible = false;
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|264fc68e-648f-409d-b8c8-6941f945035a", "Activity");
			zTextBoxColumnStyleInfo3.ColumnName = "GE_Activity";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|d718cc8d-6be8-480e-97f1-8ad75c3b9c2b", "Mode");
			zTextBoxColumnStyleInfo4.ColumnName = "GE_Mode";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|dcb46fa3-1176-48dd-bf54-22732d37d99e", "Direction");
			zTextBoxColumnStyleInfo5.ColumnName = "GE_Direction";
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|ba2f8088-ef47-4bb4-b507-b22d168463e7", "Active");
			zCheckBoxColumnStyleInfo1.ColumnName = "GE_IsActive";
			zCheckBoxColumnStyleInfo1.IsMandatory = true;
			zCheckBoxColumnStyleInfo1.IsReadOnly = true;
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("GlbDepartmentFilterControl|533fd5d0-7b06-46de-8d1c-e65f796272d9", "System");
			zCheckBoxColumnStyleInfo2.ColumnName = "GE_SystemCode";
			zCheckBoxColumnStyleInfo2.IsVisible = false;
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.FilteredGrid.TabIndex = 7;
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.GlbDepartment);
			// 
			// GlbDepartmentFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "GlbDepartmentFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
