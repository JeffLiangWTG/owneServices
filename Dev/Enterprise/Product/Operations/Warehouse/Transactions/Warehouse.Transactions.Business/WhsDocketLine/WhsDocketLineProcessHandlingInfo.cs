using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsDocketLineProcessHandlingInfo : ProcessHandlingInfo
	{
		public WhsDocketLineProcessHandlingInfo(WhsDocketLine docketLine)
			: base(docketLine)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (DocketLine.IsDeleted)
			{
				yield break;
			}

			var originalDocket = DocketLine.DocketOriginal;
			if (originalDocket == null)
			{
				yield break;
			}

			yield return new PropagationLink(originalDocket, Enumerable.Empty<WhsDocketLine>(), "Changed Hold Codes");
		}

		protected override bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			return base.IsEventLogApplicableForPropagation(logBeingAdded) && logBeingAdded.IsHoldCodeChangeEvent();
		}

		WhsDocketLine DocketLine
		{
			get { return (WhsDocketLine)LogParent; }
		}
	}
}
