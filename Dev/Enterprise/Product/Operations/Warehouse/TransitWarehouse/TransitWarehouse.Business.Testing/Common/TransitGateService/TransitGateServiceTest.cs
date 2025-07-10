using System;
using CargoWise.Application;
using Enterprise.Warehouse.Integration;
using Moq;

namespace Enterprise.Warehouse.Transit.Business.Testing
{
	public class TransitGateServiceTest : WhsTransitTestCaseWithFactory
	{
		public void TestCanInvokeGateService()
		{
			var bookingRequest = new Mock<IGateBookingRequest>();
			var validationRequest = ObjectFactory.New<ITransitBookingValidationRequest>();
			AssertExceptionThrown<NotImplementedException>(() => validationRequest.GetBooking(bookingRequest.Object));
		}
	}
}
