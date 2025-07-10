namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowExceptionTypesFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo3 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo4 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo zCalcEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCalcEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo6 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.grid)).BeginInit();
			this.grid.SuspendLayout();
			this.AddStripButton.SuspendLayout();
			this.RecentItemsPanel.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// grid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("e99d086b-df36-4f1f-8d39-e02fed8f6e9c", "Code");
			zTextBoxColumnStyleInfo1.ColumnName = "WET_Code";
			zTextBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zTextBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("8a9ed148-2f99-430a-b7f3-3efa709a2437", "Description");
			zTextBoxColumnStyleInfo2.ColumnName = "WET_DescriptionMultilingual";
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(160);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("6d23f387-24aa-446f-b290-904c0468fabe", "Category");
			zTextBoxColumnStyleInfo3.ColumnName = "WET_Category";
			zTextBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("f626afe1-1dde-4b2b-9f82-80d8f7d921dd", "System");
			zCheckBoxColumnStyleInfo1.ColumnName = "WET_IsSystem";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("a755d3a5-a910-4573-86ee-de511dd4d73a", "Active");
			zCheckBoxColumnStyleInfo2.ColumnName = "WET_IsActive";
			zCheckBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCheckBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("d2435e38-e259-4157-9389-f92578be5685", "Cause Required");
			zCheckBoxColumnStyleInfo3.ColumnName = "WET_IsCauseRequired";
			zCheckBoxColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zCheckBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("6b53a723-2085-40e2-8b4c-1835b383ea3a", "Resolution Required");
			zCheckBoxColumnStyleInfo4.ColumnName = "WET_IsResolutionRequired";
			zCheckBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(125);
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("03633aa4-015a-4dfe-b13c-affbe7e402b9", "Job Type");
			zTextBoxColumnStyleInfo4.ColumnName = "WET_JobType";
			zTextBoxColumnStyleInfo4.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(80);
			zCalcEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("13633aa4-015a-4dfe-b11c-affbe7e402be", "Default Duration (Hours)");
			zCalcEditColumnStyleInfo1.ColumnName = "WET_DefaultDurationHours";
			zCalcEditColumnStyleInfo1.Decimals = 0;
			zCalcEditColumnStyleInfo1.IsReadOnly = true;
			zCalcEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zCheckBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("2223a723-2085-40e2-8b4c-1835b383eaea", "Use Start/End Dates to calculate Duration");
			zCheckBoxColumnStyleInfo6.ColumnName = "WET_UseStartEndToCalculateDuration";
			zCheckBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(140);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo2);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo3);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.grid.ColumnStyles.Add(zCalcEditColumnStyleInfo1);
			this.grid.ColumnStyles.Add(zCheckBoxColumnStyleInfo6);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ProcessWorkflowExceptionType);
			// 
			// WorkflowExceptionTypesFilterControl
			//
			this.CaptionRenderingEnabled = true;
			this.Name = "WorkflowExceptionTypesFilterControl";
			((System.ComponentModel.ISupportInitialize)(this.grid)).EndInit();
			this.grid.ResumeLayout(false);
			this.grid.PerformLayout();
			this.AddStripButton.ResumeLayout(true);
			this.AddStripButton.PerformLayout();
			this.RecentItemsPanel.ResumeLayout(false);
			this.RecentItemsPanel.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
