using System;
using System.Collections.Generic;
using CargoWise.Types;
using Enterprise.BufferManagement.Integration;

namespace Enterprise.Workflow.Integration
{
	public interface IContainmentBarrierCreator
	{
		IEnumerable<string> TaskTypesToNotRepeat { get; set; }
		IEnumerable<string> TaskTypesToSuspendOnRepeatIfQcbTask { get; set; }
		string IterationType { get; set; }
		string QcbTaskNewStatus { get; set; }
		string QcbCreatingUserLoginName { get; set; }
		string ResourceUnderReviewNk { get; set; }

		void CreateQualityIteration(ZGuid qcbTaskPK, ZGuid iterateFromTaskPK, ZGuid reasonPK, Action<IProcessHeader> adjustTasksAfterCopyTasks = null, IEnumerable<QualityIterationTaskDescriptor> customQualityIterationTasks = null);
		ZGuid FindBestIterateFromTask(ZGuid qcbTaskPK, ContainmentBarrierIterateFromTaskSelectionMode mode, IEnumerable<string> eligibleTaskTypes);
	}
}
