using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.MasterFiles.Business
{
	static class LineTriggerProcessHandlingInfoHelper
	{
		public static IEnumerable<ProcessTask> GetMatchingLineTriggersFromParent(ProcessTask task, IStmALog logBeingAdded)
		{
			var query = new ZQuery(ProcessTasksSchema.P9_ParentID, task.P9_ParentID);
			query.AddToFilter(ProcessTasksSchema.P9_ParentTableCode, task.P9_ParentTableCode);

			var triggers = task.Factory.Load<ProcessTask>(query)
				.Where(t => t.IsWorkflowTrigger	&& t.TriggerConditions.TriggerEventCode == logBeingAdded.SL_SE_NKEvent);

			return triggers;
		}
	}
}
