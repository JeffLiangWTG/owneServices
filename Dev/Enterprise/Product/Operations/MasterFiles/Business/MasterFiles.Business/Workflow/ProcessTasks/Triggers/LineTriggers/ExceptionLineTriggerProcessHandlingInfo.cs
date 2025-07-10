using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.MasterFiles.Business
{
	public class ExceptionLineTriggerProcessHandlingInfo : ProcessHandlingInfo
	{
		public ExceptionLineTriggerProcessHandlingInfo(ProcessTask exception) : base(exception)
		{
			this.exception = exception;
		}

		readonly ProcessTask exception;

		protected override IEnumerable<IBaseTrigger> PopulateParentTriggers(IStmALog logBeingAdded)
		{
			var parentTriggers = base.PopulateParentTriggers(logBeingAdded);
			var triggers = LineTriggerProcessHandlingInfoHelper.GetMatchingLineTriggersFromParent(exception, logBeingAdded)
				.Where(t => t.P9_LineTriggerType == ProcessTasksLookups.ExceptionLineTriggerCode);

			return parentTriggers.Concat(triggers).ToList();
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			yield break;
		}

		protected override bool IsEventExcludedFromCascadingOrPropagation(ZString eventCode) => true;
	}
}
