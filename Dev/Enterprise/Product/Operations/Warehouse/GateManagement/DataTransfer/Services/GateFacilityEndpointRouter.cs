using System.Collections;
using System.Net;
using System.Web.Http;
using CargoWise.Application;
using Enterprise.Warehouse.GateManagement.Integration;

namespace Enterprise.Warehouse.GateManagement.DataTransfer
{
	public static class GateFacilityEndpointRouter
	{
		public static IGateBookingValidationResponse ValidateFacilityBooking(GateBookingValidationRequest request)
		{
			var providers = (Hashtable)ObjectFactory.Get("GateManagementFacilityEndpointManagerList");
			var objectHandle = (ObjectHandle)providers[request.FacilityTypeCode];

			return objectHandle?.GetObject() is GateManagementFacilityEndpointManager facilityEndpointManager
				? facilityEndpointManager.ValidateBooking(request)
				: throw new HttpResponseException(HttpStatusCode.NotFound);
		}
	}
}
