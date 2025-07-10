using Enterprise.TransportCommon.Business;
using Enterprise.TransportCommon.Business.Testing;

namespace Enterprise.TransportConsignment.Business.Testing
{
	internal sealed class BookingConsignmentumberGeneratorTargetTest : TransportNumberGeneratorTargetTest
	{
		protected override TransportNumberGeneratorTarget GetNumberGeneratorTargetCore()
		{
			var helper = new TransportBookingConsignmentTestHelper(Factory);
			return new BookingConsignmentNumberGeneratorTarget(helper.CreateBookingConsignment());
		}
	}
}
