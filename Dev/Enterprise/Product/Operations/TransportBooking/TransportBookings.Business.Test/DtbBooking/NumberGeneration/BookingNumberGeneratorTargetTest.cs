using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.TransportBookings.Business.Testing
{
	internal sealed class BookingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			var target = GetNumberGeneratorTargetCore();
			var registryItem = target.GetRegistryItemCore();
			Set(registryItem, "", "OSN");
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the TransportBookingNumberCustomisation", "OSN", target.NumberCustomisation);
			AssertLocation(registryItem, target.NumberCustomisationLocation);
			AssertEquals(20, target.MaxLength);
		}

		BookingNumberGeneratorTarget GetNumberGeneratorTargetCore()
		{
			var helper = new TransportBookingTestHelper(Factory);
			return new BookingNumberGeneratorTarget(helper.CreateBooking());
		}
	}
}
