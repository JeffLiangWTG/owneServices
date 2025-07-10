using System;
using Enterprise.BufferManagement.Integration;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.GUI
{
	public class WorkItemReleaseSequenceControl : ReleaseSequenceControl
	{
		WorkItem WorkItem => (WorkItem)CurrentDataItem;

		protected override IBMReleaseSequence ReleaseSequence => WorkItem?.JobWorkflow?.HighestReleaseSequence;

		protected override Type BindingDataSourceType => typeof(WorkItem);
	}
}
