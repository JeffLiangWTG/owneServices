using Enterprise.ZArchitecture;

namespace Enterprise.MasterFiles.Module
{
	public partial class WorkflowExceptionsFilterControl
	{
		private void InitializeComponent()
		{
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo zTextBoxColumnStyleInfo2 = new Enterprise.ZArchitecture.ZTextBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo zCheckBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.ZCheckBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo1 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateEditColumnStyleInfo zDateEditColumnStyleInfo2 = new Enterprise.ZArchitecture.ZDateEditColumnStyleInfo();
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
			Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo zGuidFindBoxColumnStyleInfo1 = new Enterprise.ZArchitecture.GUI.ZGuidFindBoxColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo1 = new ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo2 = new ZDateTimeOffsetEditColumnStyleInfo();
			Enterprise.ZArchitecture.ZDateTimeOffsetEditColumnStyleInfo zDateTimeOffsetEditColumnStyleInfo3 = new ZDateTimeOffsetEditColumnStyleInfo();
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).BeginInit();
			this.SuspendLayout();
			// 
			// FilteredGrid
			// 
			zTextBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|a04cb83c-3840-424d-ab34-554d6a8da99b", "Job Number");
			zTextBoxColumnStyleInfo1.ColumnName = "JobNumber";
			zTextBoxColumnStyleInfo2.ColumnName = "P9_Description";
			zTextBoxColumnStyleInfo6.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|e46a3501-2556-4658-8c25-065253b144af", "Type");
			zTextBoxColumnStyleInfo6.ColumnName = "ExceptionTypeCode";
			zTextBoxColumnStyleInfo6.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo7.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|fd9d5c95-1f8a-42f6-aa67-c048c6c7430c", "Type Description");
			zTextBoxColumnStyleInfo7.ColumnName = "ExceptionTypeDescription";
			zTextBoxColumnStyleInfo7.IsVisible = false;
			zTextBoxColumnStyleInfo7.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo12.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|806d7703-6871-4622-b171-b6cf668bb6da", "Category");
			zTextBoxColumnStyleInfo12.ColumnName = "ExceptionTypeCategory";
			zTextBoxColumnStyleInfo12.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(65);
			zTextBoxColumnStyleInfo13.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|25a01190-2f02-47c6-b957-ed3bd79cd3cb", "Category Description");
			zTextBoxColumnStyleInfo13.ColumnName = "ExceptionTypeCategoryDescription";
			zTextBoxColumnStyleInfo13.IsVisible = false;
			zTextBoxColumnStyleInfo13.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(130);
			zTextBoxColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zCheckBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|e256df74-dbb9-480d-a149-6f994ab1e5e1", "Actioned");
			zCheckBoxColumnStyleInfo1.ColumnName = "IsExceptionActioned";
			zCheckBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(50);
			zDateEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|ce60c9db-c809-474f-9099-501e01f16c1b", "ETA");
			zDateEditColumnStyleInfo1.ColumnName = "ETA";
			zDateEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|65b0cf21-8756-4c4a-942d-f2aaf2d0bfb0", "ETD");
			zDateEditColumnStyleInfo2.ColumnName = "ETD";
			zDateEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zTextBoxColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|80ba10d7-e85c-4c85-887d-57bf95b5bfe6", "Load/Origin");
			zTextBoxColumnStyleInfo3.ColumnName = "LoadOrOriginPort";
			zTextBoxColumnStyleInfo4.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|03dd8c9f-6a14-4ce5-abbf-4d77a58a06c0", "Disch./Dest.", "Discharge/Destination");
			zTextBoxColumnStyleInfo4.ColumnName = "DischargeOrDestinationPort";
			zTextBoxColumnStyleInfo5.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|2ea199f8-e184-4cd2-831f-adc02e9745f0", "Staff");
			zTextBoxColumnStyleInfo5.ColumnName = "P9_GS_NKAssignedStaffMember";
			zTextBoxColumnStyleInfo5.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo8.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|a9fe1b18-de76-4ab1-9726-e542a9b8d0e6", "Cause");
			zTextBoxColumnStyleInfo8.ColumnName = "ProcessWorkflowException.Cause.WEC_Code";
			zTextBoxColumnStyleInfo8.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zTextBoxColumnStyleInfo9.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|6f333506-acf8-46cd-bc00-48200c923aa9", "Cause Description");
			zTextBoxColumnStyleInfo9.ColumnName = "ProcessWorkflowException.Cause.WEC_Description";
			zTextBoxColumnStyleInfo9.IsVisible = false;
			zTextBoxColumnStyleInfo9.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zTextBoxColumnStyleInfo10.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|6b71a97e-7705-4c54-af59-08f6e059de1c", "Resolution");
			zTextBoxColumnStyleInfo10.ColumnName = "ProcessWorkflowException.Resolution.WER_Code";
			zTextBoxColumnStyleInfo10.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(60);
			zTextBoxColumnStyleInfo11.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|fb8ffd99-8406-478a-83c1-826979a0d6b5", "Resolution Description");
			zTextBoxColumnStyleInfo11.ColumnName = "ProcessWorkflowException.Resolution.WER_Description";
			zTextBoxColumnStyleInfo11.IsVisible = false;
			zTextBoxColumnStyleInfo11.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(120);
			zGuidFindBoxColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|ed5ee652-641b-44e2-b4cf-13e000bdd380", "Group");
			zGuidFindBoxColumnStyleInfo1.ColumnName = "P9_GG_AssignedGroup";
			zGuidFindBoxColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(40);
			zDateTimeOffsetEditColumnStyleInfo1.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|d9349bd5-7287-4afb-b746-1305686bb528", "Date");
			zDateTimeOffsetEditColumnStyleInfo1.ColumnName = "P9_ActualDateForBinding";
			zDateTimeOffsetEditColumnStyleInfo1.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo2.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|bec8b7b1-b9ff-42f3-b120-31b7e6ab2d08", "Actioned Date");
			zDateTimeOffsetEditColumnStyleInfo2.IsVisible = false;
			zDateTimeOffsetEditColumnStyleInfo2.IsReadOnly = true;
			zDateTimeOffsetEditColumnStyleInfo2.ColumnName = "CompletedTimeLocal";
			zDateTimeOffsetEditColumnStyleInfo2.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);
			zDateTimeOffsetEditColumnStyleInfo3.CaptionResourceString = Enterprise.MasterFiles.Module.Res.GetData("WorkflowExceptionsFilterControl|bd2e02fa-5639-41f8-af3a-d5c65a4df9db", "Assigned Date");
			zDateTimeOffsetEditColumnStyleInfo3.IsVisible = false;
			zDateTimeOffsetEditColumnStyleInfo3.IsReadOnly = true;
			zDateTimeOffsetEditColumnStyleInfo3.ColumnName = "ExceptionAssignedDate";
			zDateTimeOffsetEditColumnStyleInfo3.Width = CargoWise.Windows.UI.ControlDpiScalingHelper.ScaleToCurrentDpiX(90);

			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zCheckBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo6);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo7);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo12);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo13);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo3);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo4);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo5);
			this.FilteredGrid.ColumnStyles.Add(zGuidFindBoxColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo1);
			this.FilteredGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo2);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo8);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo9);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo10);
			this.FilteredGrid.ColumnStyles.Add(zTextBoxColumnStyleInfo11);
			this.FilteredGrid.ColumnStyles.Add(zDateTimeOffsetEditColumnStyleInfo3);
			this.FilteredGrid.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 264, true);
			// 
			// BindingSource
			// 
			this.BindingSource.DataSourceType = typeof(Enterprise.MasterFiles.Business.ExceptionCollectionView);
			// 
			// WorkflowExceptionsFilterControl
			// 
			this.CaptionRenderingEnabled = true;
			this.Name = "WorkflowExceptionsFilterControl";
			this.Size = CargoWise.Windows.UI.ControlDpiScalingHelper.NewScaledSize(738, 416, true);
			((System.ComponentModel.ISupportInitialize)(this.FilteredGrid)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.BindingSource)).EndInit();
			this.ResumeLayout(false);
			this.PerformLayout();
		}
	}
}
