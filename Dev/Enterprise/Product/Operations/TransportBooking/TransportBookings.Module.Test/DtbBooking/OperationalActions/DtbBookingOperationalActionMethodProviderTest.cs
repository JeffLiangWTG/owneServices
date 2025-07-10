using System;
using Enterprise.Services.OperationalActions.Support;
using Enterprise.Services.OperationalActions.Support.Testing;
using Enterprise.TransportBookings.Module.OperationalActions;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(DtbBookingOperationalActionMethodProvider))]
	public class DtbBookingOperationalActionMethodProviderTest : OperationalActionMethodProviderTest
	{
		public void TestNewMethods()
		{
			var methods = Provider.NewMethods(new DtbBookingOperationalActionSupporter());

			AssertContainsExactElementsInAnyOrder("Expected Methods", new[] {
					typeof(MakeBookingAvailableActionMethod),
					typeof(HoldBookingActionMethod),
					typeof(CreateTransportJobsOperationalActionMethod),
					typeof(CreateLandTransportConsignmentsOperationalActionMethod),
					typeof(CreatePortTransportJobsOperationalActionMethod),
					typeof(RemoveBookingHeldStatusActionMethod),
					typeof(MakeBookingStatusIncompleteActionMethod),
					typeof(CarrierBookingAgentChangeActionMethod) },
					Array.ConvertAll(methods, m => m.GetType()));

			AssertEquals(8, methods.Length);
		}

		protected override ActionMethodProviderID ID
		{
			get { return ActionMethodProviderIDs.DtbBooking; }
		}
	}
}
