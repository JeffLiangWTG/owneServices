using System.Collections.Generic;
using System.Linq;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;
using Enterprise.TransportConsignment.Integration;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.TransportConsignment.Business
{
	public class DtbConsignmentLoader : IDtbConsignmentLoader
	{
		public IEnumerable<BusinessObject> GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(IDtbBooking booking)
		{
			var childConsignmentQuery = new ZQuery(DtbConsignmentSchema.LTC_KM_Booking, booking.PK.ToGuid());
			var childConsignments = booking.Factory.Load<DtbConsignment>(childConsignmentQuery);

			var consignmentPKs = childConsignments.Select(x => x.PK);
			var addressSubQuery = new ZDBOnlySubQuery(typeof(DtbConsignmentAddress), DtbConsignmentActionSchema.LTA_LTS_ConsignmentAddress);
			addressSubQuery.AddToFilter(DtbConsignmentAddressSchema.LTS_LTC_Consignment, consignmentPKs);
			var actionQuery = new ZDBOnlyQuery(typeof(DtbConsignmentAction));
			actionQuery.AddSubQuery(addressSubQuery, JoinCondition.And);

			var actions = booking.Factory.Load<DtbConsignmentAction>(actionQuery).Cast<BusinessObject>();

			return childConsignments.Concat(actions);
		}
	}
}
