using Enterprise.Freight.Forwarding.Registry;
using Enterprise.MasterFiles.Business;
using Enterprise.MasterFiles.Business.Testing;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	sealed class SupplierBookingNumberGeneratorTargetTest : NumberGeneratorTargetTest
	{
		public void TestParameters()
		{
			Set(OrderManagerRegistry.Instance.SupplierBookingNumberFormat, "SB");

			var target = new SupplierBookingNumberGeneratorTarget();
			target.Context = new NumberGeneratorContext();

			AssertCustomisation("Should find the SupplierBookingNumberGeneratorTarget", "SB", target.NumberCustomisation);
			AssertLocation(OrderManagerRegistry.Instance.SupplierBookingNumberFormat, target.NumberCustomisationLocation);
			AssertEquals(20, target.MaxLength);
			AssertEquals("Supplier Booking Number Format", target.Name);
		}
	}
}
