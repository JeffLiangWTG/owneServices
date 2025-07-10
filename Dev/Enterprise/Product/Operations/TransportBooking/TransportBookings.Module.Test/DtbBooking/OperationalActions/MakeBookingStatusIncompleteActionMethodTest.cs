using Enterprise.Services.OperationalActions.Support.Testing;
using NUnit.Framework;

namespace Enterprise.TransportBookings.Module.Testing
{
	[TestedType(typeof(MakeBookingStatusIncompleteActionMethod))]
	public class MakeBookingStatusIncompleteActionMethodTest : OperationalActionMethodTest<MakeBookingStatusIncompleteActionMethod>
	{
		public void TestApplicatorType()
		{
			var applicator = Method.NewApplicator(Factory, null);
			AssertType(typeof(MakeBookingStatusIncompleteActionMethodApplicator), applicator);
		}

		protected override MakeBookingStatusIncompleteActionMethod NewMethod()
		{
			return new MakeBookingStatusIncompleteActionMethod();
		}
	}
}
