using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingCollection))]
	sealed class JobSupplierBookingCollectionTest : ActiveBusinessObjectCollectionTestCase<JobSupplierBookingCollection>
	{
	}
}
