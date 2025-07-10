using Enterprise.MasterFiles.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(SupplierBookingProcessTaskCollection))]
	sealed class SupplierBookingProcessTaskCollectionTest : ProcessTaskCollectionTest<SupplierBookingProcessTaskCollection>
	{
		#region Implementation

		protected override SupplierBookingProcessTaskCollection GetCollectionToTestCore()
		{
			return new SupplierBookingProcessTaskCollection(Factory.NewWithValidTestData<JobSupplierBooking>());
		}

		#endregion
	}
}
