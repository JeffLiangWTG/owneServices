using System.Collections.Generic;
using Enterprise.Freight.Business;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.Freight.Agency.Business
{
	public class AgencyShipmentContainerProcessHandlingInfo : CommonContainerProcessHandlingInfo
	{
		public AgencyShipmentContainerProcessHandlingInfo(AgencyShipmentContainer container)
			: base(container)
		{
			this.container = container;
		}

		readonly AgencyShipmentContainer container;

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return null;
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			var links = new List<PropagationLink>();

			if (!container.IsDeleted
				&& container.Booking != null
				&& container.Booking.IsBillOfLadingStage
				&& container.Booking.ShippingContainers.Contains(container))
			{
				links.Add(new PropagationLink(container.Booking, container.Booking.ShippingContainers, "Containers"));
			}

			return links;
		}
	}
}
