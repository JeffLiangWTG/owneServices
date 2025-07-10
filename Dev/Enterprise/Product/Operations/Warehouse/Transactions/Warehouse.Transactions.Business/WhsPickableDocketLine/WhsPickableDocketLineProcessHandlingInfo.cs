using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsPickableDocketLineProcessHandlingInfo : WhsDocketLineProcessHandlingInfo
	{
		public WhsPickableDocketLineProcessHandlingInfo(WhsPickableDocketLine docketLine)
			: base(docketLine)
		{
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			return Enumerable.Empty<PropagationLink>();
		}

		protected override bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			return false;
		}
	}
}
