using CargoWise.EntityFramework;
using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Business.Testing
{
	[TestedType(typeof(ForwardingShipment.BookingContainerCollectionForBinding))]
	sealed class BookingContainerCollectionForBindingTest : BusinessObjectCollectionTestCase
	{
		protected override BusinessObjectCollection GetCollectionToTest()
		{
			var shipment = Factory.New<ForwardingShipment>();
			return new ForwardingShipment.BookingContainerCollectionForBinding(shipment, Factory);
		}
	}
}
