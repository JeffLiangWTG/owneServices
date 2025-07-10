using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.ProcessManagement.Business
{
	class WorkItemProcessHandlingInfo : ProcessHandlingInfo
	{
		internal WorkItemProcessHandlingInfo(WorkItem workItem)
			: base(workItem)
		{
			this.workItem = workItem;
		}

		readonly WorkItem workItem;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			yield break;
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			var workRequests = workItem.RelatedItems.OfType<WorkRequest>().ToArray();

			foreach (var request in workRequests)
			{
				yield return new PropagationLink(request, request.RelatedItems, "Work Items");
			}
		}

		protected override bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			return base.IsEventLogApplicableForPropagation(logBeingAdded)
				&& logBeingAdded.SL_SE_NKEvent == AutoEvents.JobCloseCode;
		}
	}
}
