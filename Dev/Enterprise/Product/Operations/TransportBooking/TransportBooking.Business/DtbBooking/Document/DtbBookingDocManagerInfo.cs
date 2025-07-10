using System;
using System.Collections.Generic;
using System.Linq;
using CargoWise.Application;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Core;
using Enterprise.MasterFiles.Business;
using Enterprise.TransportConsignment.Integration;

namespace Enterprise.TransportBookings.Business
{
	public class DtbBookingDocManagerInfo : DocManagerInfo
	{
		public DtbBookingDocManagerInfo(DtbBooking booking)
			: base(booking, Constants.DocManagerCodes.DomesticTransportBooking)
		{
		}

		protected override BusinessObject[] GetRelatedObjects()
		{
			var result = new List<BusinessObject>(base.GetRelatedObjects());

			AddOrganisationIfValid(result, Booking.Address);
			Array.ForEach(Booking.Instructions.ToArray(), i => AddOrganisationIfValid(result, i.Address));

			var consignmentLoader = ObjectFactory.Get<IDtbConsignmentLoader>();
			result.AddRange(consignmentLoader.GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(Booking));

			var bookingConsolidation = Booking.ConsolidationSingleJob;
			if (bookingConsolidation != null)
			{
				var parent = bookingConsolidation.Parent;
				if (parent?.ParentWithWorkflow != null)
				{
					result.Add(parent.ParentWithWorkflow);
				}
			}

			if (Booking.KM_KM_MasterBooking != ZGuid.Empty)
			{
				result.Add(Booking.MasterBooking);
			}
			if (Booking.KM_IsMaster)
			{
				result.AddRange(Booking.SubBookings);
			}

			result.AddRange(Booking.GetPortTransportJobs().OrderBy(pt => pt.JJ_ConsignmentID).Cast<BusinessObject>());

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

		public override bool ReadOnly
		{
			get { return false; }
		}

		DtbBooking Booking
		{
			get { return (DtbBooking)BusinessEntity; }
		}
	}
}
