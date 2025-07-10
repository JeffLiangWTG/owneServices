using System.Collections.Generic;
using CargoWise.EntityFramework;
using Enterprise.Integration.TransportBooking;

namespace Enterprise.TransportConsignment.Integration
{
	public interface IDtbConsignmentLoader
	{
		IEnumerable<BusinessObject> GetRelatedTransportConsignmentsAndChildrenForLocatingEDocs(IDtbBooking booking);
	}
}
