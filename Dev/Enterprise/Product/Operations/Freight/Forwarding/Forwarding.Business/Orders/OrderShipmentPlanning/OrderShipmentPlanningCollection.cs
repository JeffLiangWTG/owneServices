using CargoWise.EntityFramework;
using Enterprise.ZArchitecture.Schema;

namespace Enterprise.Freight.Forwarding.Orders.Business
{
	public class OrderShipmentPlanningCollection : ActiveBusinessObjectCollection<OrderShipmentPlanning>
	{
		public OrderShipmentPlanningCollection(JobSupplierBooking master) : base(master.Factory, master, null, OrderShipmentPlanningSchema.OPS_JSB_Booking)
		{
		}
	}
}
