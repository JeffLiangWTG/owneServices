using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class WorkItemDetailsControl : ZUserControl
	{
		public WorkItemDetailsControl()
		{
			InitializeComponent();
		}

		WorkItem WorkItem
		{
			get { return (WorkItem)CurrentDataItem; }
		}

		protected override void OnCurrentDataItemChanged(EventArgs e)
		{
			base.OnCurrentDataItemChanged(e);
			if (WorkItem != null)
			{
				CustomLabelHelper.Setup(WorkItemTypeDropEdit, ProcessManagementRegistry.Instance.WorkItemTypeLabel);
				CustomLabelHelper.Setup(WorkItemAreaDropEdit, ProcessManagementRegistry.Instance.WorkItemAreaLabel);
				CustomLabelHelper.Setup(ActivityTypeDropEdit, ProcessManagementRegistry.Instance.WorkItemActivityTypeLabel);
				CustomLabelHelper.Setup(ActivitySubTypeDropEdit, ProcessManagementRegistry.Instance.WorkItemActivitySubTypeLabel);
				CustomLabelHelper.Setup(PriorityDropEdit, ProcessManagementRegistry.Instance.WorkItemPriorityLabel);
			}
		}

		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			ActiveControl = WorkItemTypeDropEdit;
		}
	}
}
