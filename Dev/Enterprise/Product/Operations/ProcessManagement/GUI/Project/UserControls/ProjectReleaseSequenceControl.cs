using System;
using Enterprise.BufferManagement.Integration;
using Enterprise.ProcessManagement.Business;

namespace Enterprise.ProcessManagement.GUI
{
	public class ProjectReleaseSequenceControl : ReleaseSequenceControl
	{
		Project Project => (Project)CurrentDataItem;
		protected override IBMReleaseSequence ReleaseSequence => Project?.JobWorkflow?.HighestReleaseSequence;
		protected override Type BindingDataSourceType => typeof(Project);
	}
}
