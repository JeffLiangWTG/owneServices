using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(HoldBookingActionMethod))]
	public class HoldBookingActionMethodTest : OperationalActionMethodTest<HoldBookingActionMethod>
	{
		public void TestApplicatorType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(HoldBookingChangeActionMethodApplicator), applicator);
		}

		protected override HoldBookingActionMethod NewMethod()
		{
			return new HoldBookingActionMethod();
		}
	}
}
