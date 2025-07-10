using CargoWise.EntityFramework;
using Enterprise.Freight.Forwarding.Business;
using Enterprise.ZArchitecture.Business.Testing;
using NUnit.Framework;

namespace Enterprise.Freight.Forwarding.Orders.Business.Testing;

[TestedType(typeof(OrderShipmentPlanningLine))]
public class OrderShipmentPlanningLineTest : EnterpriseBusinessObjectTestCase
{
	protected override BusinessObject GetNewBusinessObject()
	{
		return GetOrderShipmentPlanningLineObject(Factory);
	}

	protected override BusinessObject GetNewBusinessObjectForDeleteTest(BusinessObjectFactory factory)
	{
		return GetOrderShipmentPlanningLineObject(factory);
	}

	OrderShipmentPlanningLine GetOrderShipmentPlanningLineObject(BusinessObjectFactory factory)
	{
		var supplierBookingLine = factory.NewWithValidTestData<JobSupplierBookingLine>();
		var orderShipmentPlanning = factory.NewWithValidTestData<OrderShipmentPlanning>();
		orderShipmentPlanning.OPS_JSB_Booking = supplierBookingLine.JSL_JSB_Booking;

		var orderShipmentPlanningLine = factory.NewWithValidTestData<OrderShipmentPlanningLine>();
		orderShipmentPlanningLine.OPL_OPS_Planning = orderShipmentPlanning.PK;
		orderShipmentPlanningLine.OPL_JSL_BookingLine = supplierBookingLine.PK;

		return orderShipmentPlanningLine;
	}

	public void TestPackLine()
	{
		var shipment = Factory.NewWithValidTestData<ForwardingShipment>();
		var packLine = shipment.OuterPackLines.AddNew();

		var booking = Factory.NewWithValidTestData<JobSupplierBooking>();
		var shipmentPlanning = booking.OrderShipmentPlannings.AddNew();
		var planningLine = shipmentPlanning.OrderShipmentPlanningLines.AddNew();
		planningLine.OPL_JL_PackLine = packLine.PK;

		AssertEquals(packLine.PK, planningLine.PackLine.PK);
	}
}

