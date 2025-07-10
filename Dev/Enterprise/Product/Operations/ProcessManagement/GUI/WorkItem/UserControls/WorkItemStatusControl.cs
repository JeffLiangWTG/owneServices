using System;
using Enterprise.ProcessManagement.Business;
using Enterprise.Registry.Business;
using Enterprise.ZArchitecture.Core;
using Enterprise.ZArchitecture.GUI;

namespace Enterprise.ProcessManagement.GUI
{
	public partial class WorkItemStatusControl : ZUserControl
	{
		public WorkItemStatusControl()
		{
			InitializeComponent();

			if (!DesignModeFinder.IsDesigning)
			{
				var colorTheme = SystemDataRegistry.Instance.ColorTheme;
				TaskStatusBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				TaskAssignedStaffBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
				CreatedTimeBox.ColorChanger.ForceBackColor(colorTheme.TabBackgroundColor);
			}
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
				CustomLabelHelper.Setup(DefectIntroducedInWorkItemGuidFindBox, ProcessManagementRegistry.Instance.DefectIntroducedInWorkItemLabel);
				CustomLabelHelper.Setup(DefectIntroducedInTaskGuidDropEdit, ProcessManagementRegistry.Instance.DefectIntroducedInTaskLabel);
				CustomLabelHelper.Setup(FirstCBThatMissedDefectGuidDropEdit, ProcessManagementRegistry.Instance.FirstCBThatMissedDefectLabel);
			}
		}
	}
}
