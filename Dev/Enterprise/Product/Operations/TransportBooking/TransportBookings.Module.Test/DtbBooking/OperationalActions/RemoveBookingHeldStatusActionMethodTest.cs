using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(RemoveBookingHeldStatusActionMethod))]
	public class RemoveBookingHeldStatusActionMethodTest : OperationalActionMethodTest<RemoveBookingHeldStatusActionMethod>
	{
		public void TestApplicatorType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(RemoveBookingHeldStatusActionMethodApplicator), applicator);
		}

		protected override RemoveBookingHeldStatusActionMethod NewMethod()
		{
			return new RemoveBookingHeldStatusActionMethod();
		}
	}
}
