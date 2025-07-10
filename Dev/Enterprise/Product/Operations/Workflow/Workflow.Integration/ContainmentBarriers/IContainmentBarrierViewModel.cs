using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IContainmentBarrierViewModel : IDisposable
	{
		ZGuid IterateFromTaskPK { get; set; }
		ZGuid IterateReasonPK { get; set; }
		ZString ResourceUnderReviewNK { get; set; }
		ContainmentBarrierResponses? Response { get; set; }

		void CommitResponse(Action<IProcessHeader> adjustTasksAfterCopyTasks = null, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks = null);
	}
}
