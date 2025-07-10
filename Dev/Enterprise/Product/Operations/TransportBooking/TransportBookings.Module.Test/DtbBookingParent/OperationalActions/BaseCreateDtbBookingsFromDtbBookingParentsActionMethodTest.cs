using System;
using Enterprise.Services.OperationalActions.Support.Testing;

namespace Enterprise.TransportBookings.Module.Testing
{
	abstract class BaseCreateDtbBookingsFromDtbBookingParentsActionMethodTest<MethodT> : OperationalActionMethodTest<MethodT> where MethodT : BaseCreateDtbBookingsFromDtbBookingParentsActionMethod
	{
		public void TestNameAndDescription()
		{
			AssertEquals(ExpectedNameAndDescription, Method.Name);
			AssertEquals(ExpectedNameAndDescription, Method.Description);
		}

		protected abstract string ExpectedNameAndDescription { get; }

		public void TestHasControl()
		{
			AssertEquals("Should have control enabled", true, Method.HasControl);
		}

		public void TestDoesNotHaveSettings()
		{
			AssertEquals("Should have settings disabled", false, Method.HasSettings);
		}

		public void TestNewApplicatorType()
		{
			AssertEquals(ExpectedApplicatorType, Method.NewApplicator(Factory, null).GetType());
		}

		protected abstract Type ExpectedApplicatorType { get; }
	}
}
