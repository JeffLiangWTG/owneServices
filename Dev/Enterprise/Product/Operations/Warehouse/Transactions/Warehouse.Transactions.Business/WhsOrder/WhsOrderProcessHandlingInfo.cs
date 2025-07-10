using System.Collections.Generic;
using System.Linq;
using CargoWise.Types;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Warehouse.Transactions.Business
{
	class WhsOrderProcessHandlingInfo : ProcessHandlingInfo
	{
		public WhsOrderProcessHandlingInfo(WhsOrder order)
			: base(order)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			var pick = Order.Pick;
			if (pick == null)
			{
				yield break;
			}

			yield return new PropagationLink(pick, pick.Orders.Where(o => o.PK != Order.PK), "Orders");
		}

		protected override bool IsEventLogApplicableForPropagation(IStmALog logBeingAdded)
		{
			return base.IsEventLogApplicableForPropagation(logBeingAdded) && PropagatedEvents.Contains(logBeingAdded.SL_SE_NKEvent.ToUpperInvariant());
		}

		static ZString[] PropagatedEvents => new ZString[] { Events.PackingCompletedCode, Events.PickedUpCode, Events.FreightLoadedCode, Events.DepartureCode };

		WhsOrder Order
		{
			get { return (WhsOrder)LogParent; }
		}
	}
}
