using CargoWise.EntityFramework.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing
{
	[TestedType(typeof(OrderShipmentPlanningLineCollection))]
	internal class OrderShipmentPlanningLineCollectionTest : ActiveBusinessObjectCollectionTestCase<OrderShipmentPlanningLineCollection>
	{
		protected override OrderShipmentPlanningLineCollection GetCollectionToTest()
		{
			var supplierBooking = Factory.New<JobSupplierBooking>();
			return supplierBooking.OrderShipmentPlannings.AddNew().OrderShipmentPlanningLines;
		}
	}
}
