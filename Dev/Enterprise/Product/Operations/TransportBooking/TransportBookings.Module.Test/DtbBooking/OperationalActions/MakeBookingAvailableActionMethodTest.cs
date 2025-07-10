using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(MakeBookingAvailableActionMethod))]
	public class MakeBookingAvailableActionMethodTest : OperationalActionMethodTest<MakeBookingAvailableActionMethod>
	{
		public void TestApplicatorType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(MakeBookingAvailableChangeActionMethodApplicator), applicator);
		}

		protected override MakeBookingAvailableActionMethod NewMethod()
		{
			return new MakeBookingAvailableActionMethod();
		}
	}
}
