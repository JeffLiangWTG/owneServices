using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using Enterprise.Freight.Forwarding.Business;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(JobSupplierBookingContainerCollection))]
	public class JobSupplierBookingContainerCollectionTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var supplierBooking = Factory.NewWithValidTestData<JobSupplierBooking>();
			var consol = Factory.NewWithValidTestData<ForwardingConsol>();
			var container1 = Factory.NewWithValidTestData<ForwardingContainer>();
			container1.JC_JSB_SupplierBooking = supplierBooking.PK;
			var container2 = Factory.NewWithValidTestData<ForwardingContainer>();
			container2.JC_JSB_SupplierBooking = supplierBooking.PK;
			consol.Containers.Add(container1);
			consol.Containers.Add(container2);

			return new JobSupplierBookingContainerCollection(supplierBooking);
		}
	}
}

