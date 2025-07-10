using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing;

[TestedType(typeof(OrderShipmentPlanning))]
public class OrderShipmentPlanningTest : EnterpriseBusinessObjectTestCase
{
	#region Properties

	public void TestShipment()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
		var orderShipmentPlanning = booking.OrderShipmentPlannings.AddNew();
		orderShipmentPlanning.OPS_JS_Shipment = shipment.PK;

		AssertEquals(shipment.PK, orderShipmentPlanning.Shipment.PK);
	}

	public void TestSupplierBooking()
	{
		var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
		var planning = booking.OrderShipmentPlannings.AddNew();

		AssertEquals(booking.PK, planning.SupplierBooking.PK);
	}

	public void TestDelete()
	{
		var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
		var planning = booking.OrderShipmentPlannings.AddNew();
		var planningLine1 = planning.OrderShipmentPlanningLines.AddNew();
		var planningLine2 = planning.OrderShipmentPlanningLines.AddNew();
		planning.Delete();

		AssertEquals(expected: true, planningLine1.IsDeleted);
		AssertEquals(expected: true, planningLine2.IsDeleted);
	}

	#endregion

	#region Implementation

	protected override BusinessObject GetNewBusinessObject()
	{
		return GetOrderShipmentPlanningObject(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetOrderShipmentPlanningObject(factory);
	}

	OrderShipmentPlanning GetOrderShipmentPlanningObject(BusinessObjectFactory factory)
	{
		var supplierBooking = factory.NewWithValidTestData<JobSupplierBooking>();
		var orderShipmentPlanning = factory.NewWithValidTestData<OrderShipmentPlanning>();
		orderShipmentPlanning.OPS_JSB_Booking = supplierBooking.PK;
		return orderShipmentPlanning;
	}

	#endregion
}

