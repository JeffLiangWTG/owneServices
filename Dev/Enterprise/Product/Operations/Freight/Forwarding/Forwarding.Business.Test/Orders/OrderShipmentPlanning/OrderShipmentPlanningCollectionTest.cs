using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderShipmentPlanningCollection))]
	internal class OrderShipmentPlanningCollectionTest : ActiveBusinessObjectCollectionTestCase<OrderShipmentPlanningCollection>
	{
		protected override OrderShipmentPlanningCollection GetCollectionToTest()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			return supplierBooking.OrderShipmentPlannings;
		}
	}
}
