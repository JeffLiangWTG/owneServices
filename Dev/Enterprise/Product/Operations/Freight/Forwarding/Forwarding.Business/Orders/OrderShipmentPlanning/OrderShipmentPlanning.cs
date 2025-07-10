using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.Orders.Business;

[CodeAlive(reason: "Will be used by WI00785406 - GLWOM: Spliting SBK line within SPT")]
public class OrderShipmentPlanning : AutoOrderShipmentPlanning
{
	public OrderShipmentPlanning(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[RelatedBusinessObject("SupplierBooking")]
	public override ZGuid OPS_JSB_Booking { get => base.OPS_JSB_Booking; set => base.OPS_JSB_Booking = value; }

	[RelatedBusinessObject("Shipment")]
	public override ZGuid OPS_JS_Shipment { get => base.OPS_JS_Shipment; set => base.OPS_JS_Shipment = value; }

	public JobSupplierBooking SupplierBooking => supplierBooking ??= Factory.Load<JobSupplierBooking>(OPS_JSB_Booking);
	JobSupplierBooking supplierBooking;

	public ForwardingShipment Shipment => shipment ??= Factory.Load<ForwardingShipment>(OPS_JS_Shipment);
	ForwardingShipment shipment;

	public OrderShipmentPlanningLineCollection OrderShipmentPlanningLines => orderShipmentPlanningLines ??= new OrderShipmentPlanningLineCollection(this);
	OrderShipmentPlanningLineCollection orderShipmentPlanningLines;

	public override void Delete()
	{
		OrderShipmentPlanningLines.DeleteAll();

		base.Delete();
	}
}
