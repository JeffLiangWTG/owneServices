using System;
using Enterprise.Warehouse.Integration;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Warehouse.Transit.Business
{
	[CodeAlive("Transit Gate Service will be implemented in later work items")]
	public class TransitGateService : ITransitBookingValidationRequest
	{
		public ITransitBookingInfo GetBooking(IGateBookingRequest request)
		{
			throw new NotImplementedException();
		}
	}
}
