using System.Collections.Generic;
using System.Linq;
using Enterprise.Integration;
using Enterprise.ZArchitecture.Business.Business.EventManagement;
using Enterprise.ZArchitecture.Business.EventManagement;

namespace Enterprise.TransportConsignment.Business
{
	class DtbBookingConsignmentProcessHandlingInfo : ProcessHandlingInfo
	{
		public DtbBookingConsignmentProcessHandlingInfo(DtbBookingConsignment consignment)
			: base(consignment)
		{
		}

		protected override IEnumerable<CascadingLink> PopulateCascadingTargets(IStmALog logBeingAdded)
		{
			return Enumerable.Empty<CascadingLink>();
		}

		protected override IEnumerable<PropagationLink> PopulatePropagationTargets()
		{
			if (Consignment.IsDeleted)
			{
				yield break;
			}

			var consignmentConsol = Consignment.ConsolidationSingleJob;
			if (consignmentConsol == null)
			{
				yield break; // should not actually happen
			}

			var parentBooking = consignmentConsol.Parent;
			if (parentBooking == null)
			{
				yield break;
			}

			yield return new PropagationLink(parentBooking, consignmentConsol.Bookings, "Consignments");
		}

		DtbBookingConsignment Consignment
		{
			get { return (DtbBookingConsignment)LogParent; }
		}
	}
}
