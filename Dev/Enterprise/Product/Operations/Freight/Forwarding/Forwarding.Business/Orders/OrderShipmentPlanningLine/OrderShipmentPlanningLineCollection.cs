using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderShipmentPlanningLineCollection : ActiveBusinessObjectCollection<OrderShipmentPlanningLine>
	{
		public OrderShipmentPlanningLineCollection(OrderShipmentPlanning master) : base(master.Factory, master, null, OrderShipmentPlanningLineSchema.OPL_OPS_Planning)
		{
		}
	}
}
