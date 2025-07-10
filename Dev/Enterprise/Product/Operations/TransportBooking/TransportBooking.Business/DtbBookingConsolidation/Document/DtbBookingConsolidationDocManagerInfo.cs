using System.Collections.Generic;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.MasterFiles.Business;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingConsolidationDocManagerInfo : DocManagerInfo
	{
		public DtbBookingConsolidationDocManagerInfo(DtbBookingConsolidation transportBooking, ZString docManagerCode)
			: base(transportBooking, docManagerCode)
		{
			TransportBooking = transportBooking;
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());
			result.AddRange(TransportBooking.Bookings);

			if (TransportBooking.IsMultiBooking)
			{
				AddOrganisationIfValid(result, TransportBooking.Address);
			}
			else
			{
				var parent = TransportBooking.Parent;
				if (parent?.ParentWithWorkflow != null)
				{
					result.Add(parent.ParentWithWorkflow);
				}
			}

			return result.ToArray();
		}

		void AddOrganisationIfValid(List<BusinessObject> relatedObjects, JobDocAddress address)
		{
			if (address != null)
			{
				var organisation = address.Organisation;
				if (organisation != null)
				{
					relatedObjects.Add(organisation);
				}
			}
		}

		readonly DtbBookingConsolidation TransportBooking;
	}
}
