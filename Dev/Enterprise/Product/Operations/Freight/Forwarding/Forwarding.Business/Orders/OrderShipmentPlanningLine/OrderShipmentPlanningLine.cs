using System.Data;
using CargoWise.EntityFramework;
using CargoWise.Types;
using Enterprise.Freight.Forwarding.Business;
using WTG.StaticAnalysis.Annotation;

namespace Enterprise.Freight.Forwarding.Orders.Business;

[CodeAlive(reason: "Will be used by WI00785406 - GLWOM: Spliting SBK line within SPT")]
public class OrderShipmentPlanningLine : AutoOrderShipmentPlanningLine
{
	public OrderShipmentPlanningLine(BusinessObjectFactory factory, DataRow row) : base(factory, row)
	{
	}

	[RelatedBusinessObject("SupplierBookingLine")]
	public override ZGuid OPL_JSL_BookingLine { get => base.OPL_JSL_BookingLine; set => base.OPL_JSL_BookingLine = value; }

	public JobSupplierBookingLine SupplierBookingLine => supplierBookingLine ??= Factory.Load<JobSupplierBookingLine>(OPL_JSL_BookingLine);
	JobSupplierBookingLine supplierBookingLine;

	[RelatedBusinessObject("OrderShipmentPlanning")]
	public override ZGuid OPL_OPS_Planning { get => base.OPL_OPS_Planning; set => base.OPL_OPS_Planning = value; }

	public OrderShipmentPlanning OrderShipmentPlanning => orderShipmentPlanning ??= Factory.Load<OrderShipmentPlanning>(OPL_OPS_Planning);
	OrderShipmentPlanning orderShipmentPlanning;

	public ForwardingPackLine PackLine => packLine ??= Factory.Load<ForwardingPackLine>(OPL_JL_PackLine);
	ForwardingPackLine packLine;
}

