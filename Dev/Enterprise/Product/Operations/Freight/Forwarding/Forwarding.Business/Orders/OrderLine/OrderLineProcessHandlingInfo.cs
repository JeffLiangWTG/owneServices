using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	class OrderLineProcessHandlingInfo : ProcessHandlingInfo
	{
		public OrderLineProcessHandlingInfo(OrderLine orderLine) : base(orderLine)
		{
			this.orderLine = orderLine;
		}
		readonly OrderLine orderLine;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return null;
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			var links = new List<PropagationLink>();

			if (!orderLine.IsDeleted && orderLine.Order != null)
			{
				var query = new ZQuery(JobOrderLineSchema.JO_JD, orderLine.JO_JD);
				var siblings = orderLine.Factory.Load<OrderLine>(query);
				links.Add(new PropagationLink(orderLine.Order, siblings, "Order OrderLines"));
			}

			return links.ToArray();
		}
	}
}
