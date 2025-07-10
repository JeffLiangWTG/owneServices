using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.MasterFiles.Business
{
	class TaskLineTriggerProcessHandlingInfo : ProcessHandlingInfo
	{
		internal TaskLineTriggerProcessHandlingInfo(ProcessTask task)
			: base(task)
		{
			this.task = task;
		}

		readonly ProcessTask task;

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var parentTriggers = base.PopulateParentTriggers(logBeingAdded);
			var triggers = LineTriggerProcessHandlingInfoHelper.GetMatchingLineTriggersFromParent(task, logBeingAdded)
				.Where(t => t.P9_LineTriggerType == ProcessTasksLookups.TaskLineTriggerCode);

			return parentTriggers.Concat(triggers).ToList();
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			yield break;
		}

		protected override bool IsEventExcludedFromCascadingOrPropagation(ZString eventCode) => true;
	}
}
