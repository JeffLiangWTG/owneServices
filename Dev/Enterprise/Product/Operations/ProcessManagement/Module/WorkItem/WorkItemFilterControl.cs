using CargoWise.EntityFramework;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.GUI;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.Module
{
	public partial class WorkItemFilterControl : ZFilterStripControl
	{
		public WorkItemFilterControl(IBusinessObjectCollection gridCollection, FilterStripBusinessObject filterBusinessObject)
			: base(gridCollection, filterBusinessObject)
		{
			InitializeComponent();
			if (!DesignModeFinder.IsDesigning)
			{
				SetCustomLabels();
				if (GridCollection != null)
				{
					WorkflowCustomFieldsGridReadonlyInitializer.AddWorkflowCustomFieldsColumns(FilteredGrid, GridCollection, JobInvoicingConsumerTypes.WorkItem.Code);
				}
			}
		}

		void SetColumnStyles(ZTextBoxColumnStyleInfo textBoxColumnStyle)
		{
			switch (textBoxColumnStyle.ColumnName)
			{
				case AutoWorkItem.Schema.WKI_WorkItemType:
				case WorkItemCommon.Schema.WorkItemTypeDescription:
					textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.WorkItemTypeLabel.Value;
					break;
				case AutoWorkItem.Schema.WKI_WorkItemArea:
				case WorkItemCommon.Schema.AreaDescription:
					textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.WorkItemAreaLabel.Value;
					break;
				case AutoWorkItem.Schema.WKI_ActivityType:
				case WorkItemCommon.Schema.ActivityTypeDescription:
					textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel.Value;
					break;
				case AutoWorkItem.Schema.WKI_ActivitySubtype:
				case WorkItemCommon.Schema.ActivitySubtypeDescription:
					textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel.Value;
					break;
				case AutoWorkItem.Schema.WKI_Priority:
				case WorkItemCommon.Schema.PriorityDescription:
					textBoxColumnStyle.Caption = ProcessManagementRegistry.Instance.WorkItemPriorityLabel.Value;
					break;
			}
		}

		void SetCustomLabels()
		{
			foreach (var columnStyle in grid.ColumnStyles)
			{
				if (columnStyle is ZTextBoxColumnStyleInfo textBoxColumnStyle)
				{
					SetColumnStyles(textBoxColumnStyle);
				}
			}
		}
	}
}
